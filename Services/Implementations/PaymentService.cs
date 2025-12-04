// File: Services/Implementations/PaymentService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotelManagementSystem.Data;
using HotelManagementSystem.Models.Entities;
using HotelManagementSystem.Models.ViewModels.Payment;
using HotelManagementSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelManagementSystem.Services.Implementations
{
    public class PaymentService : IPaymentService
    {
        private readonly HotelDbContext _dbContext;

        public PaymentService(HotelDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<PaymentViewModel>> GetAllAsync(
            string? status = null,
            string? method = null,
            long? bookingId = null)
        {
            var query = _dbContext.Payments
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Guest)
                .Include(p => p.Booking.BookingRooms)
                    .ThenInclude(br => br.Room)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
                query = query.Where(p => p.Status == status);

            if (!string.IsNullOrEmpty(method))
                query = query.Where(p => p.Method == method);

            if (bookingId.HasValue)
                query = query.Where(p => p.BookingId == bookingId.Value);

            var payments = await query
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return payments.Select(MapToViewModel).ToList();
        }

        public async Task<PaymentViewModel?> GetByIdAsync(long id)
        {
            var payment = await _dbContext.Payments
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Guest)
                .Include(p => p.Booking.BookingRooms)
                    .ThenInclude(br => br.Room)
                .FirstOrDefaultAsync(p => p.Id == id);

            return payment == null ? null : MapToViewModel(payment);
        }

        public async Task<List<PaymentViewModel>> GetByBookingIdAsync(long bookingId)
        {
            var payments = await _dbContext.Payments
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Guest)
                .Include(p => p.Booking.BookingRooms)
                    .ThenInclude(br => br.Room)
                .Where(p => p.BookingId == bookingId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return payments.Select(MapToViewModel).ToList();
        }

        public async Task<long> CreatePaymentAsync(long bookingId, string method, decimal amount)
        {
            var booking = await _dbContext.Bookings.FindAsync(bookingId);
            if (booking == null)
                throw new InvalidOperationException("Booking không tồn tại");

            if (booking.Status == "Cancelled")
                throw new InvalidOperationException("Không thể tạo payment cho booking đã hủy");

            if (amount <= 0)
                throw new ArgumentException("Số tiền phải lớn hơn 0");

            var payment = new Payment
            {
                BookingId = bookingId,
                Method = method,
                Amount = amount,
                Status = "Unpaid",
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.Payments.Add(payment);
            await _dbContext.SaveChangesAsync();

            return payment.Id;
        }

        public async Task<(bool Success, string Message, string? TxnCode)> ProcessMockPaymentAsync(long paymentId)
        {
            var payment = await _dbContext.Payments
                .Include(p => p.Booking)
                .FirstOrDefaultAsync(p => p.Id == paymentId);

            if (payment == null)
                return (false, "Payment không tồn tại", null);

            if (payment.Status == "Paid")
                return (false, "Payment đã được thanh toán", payment.TxnCode);

            if (payment.Status == "Refunded")
                return (false, "Payment đã được hoàn tiền", null);

            if (payment.Method != "Mock")
                return (false, "Payment này không phải phương thức Mock", null);

            // Simulate payment processing
            await Task.Delay(1000); // Giả lập thời gian xử lý

            // Generate mock transaction code
            var txnCode = $"MOCK{DateTime.UtcNow.Ticks}";
            
            payment.Status = "Paid";
            payment.TxnCode = txnCode;

            // Update booking payment status
            if (payment.Booking != null)
            {
                var totalPaid = await GetTotalPaidAmountAsync(payment.BookingId);
                var bookingTotal = payment.Booking.TotalAmount - payment.Booking.DiscountAmount;

                if (totalPaid >= bookingTotal)
                {
                    payment.Booking.PaymentStatus = "Paid";
                }
            }

            await _dbContext.SaveChangesAsync();

            return (true, "Thanh toán thành công", txnCode);
        }

        public async Task<(bool Success, string Message)> ProcessPayAtPropertyAsync(long paymentId)
        {
            var payment = await _dbContext.Payments
                .Include(p => p.Booking)
                .FirstOrDefaultAsync(p => p.Id == paymentId);

            if (payment == null)
                return (false, "Payment không tồn tại");

            if (payment.Status == "Paid")
                return (false, "Payment đã được thanh toán");

            if (payment.Status == "Refunded")
                return (false, "Payment đã được hoàn tiền");

            if (payment.Method != "PayAtProperty")
                return (false, "Payment này không phải phương thức PayAtProperty");

            payment.Status = "Paid";
            payment.TxnCode = $"PROP{DateTime.UtcNow.Ticks}";

            // Update booking payment status
            if (payment.Booking != null)
            {
                var totalPaid = await GetTotalPaidAmountAsync(payment.BookingId);
                var bookingTotal = payment.Booking.TotalAmount - payment.Booking.DiscountAmount;

                if (totalPaid >= bookingTotal)
                {
                    payment.Booking.PaymentStatus = "Paid";
                }
            }

            await _dbContext.SaveChangesAsync();

            return (true, "Xác nhận thanh toán tại khách sạn thành công");
        }

        public async Task<(bool Success, string Message)> ProcessRefundAsync(long paymentId, string reason)
        {
            var payment = await _dbContext.Payments
                .Include(p => p.Booking)
                .FirstOrDefaultAsync(p => p.Id == paymentId);

            if (payment == null)
                return (false, "Payment không tồn tại");

            if (payment.Status != "Paid")
                return (false, "Chỉ có thể hoàn tiền cho payment đã thanh toán");

            // Simulate refund processing
            await Task.Delay(500);

            payment.Status = "Refunded";

            // Update booking payment status
            if (payment.Booking != null)
            {
                // Check if any other payments are still paid
                var hasOtherPaidPayments = await _dbContext.Payments
                    .AnyAsync(p => 
                        p.BookingId == payment.BookingId && 
                        p.Id != paymentId && 
                        p.Status == "Paid");

                if (!hasOtherPaidPayments)
                {
                    payment.Booking.PaymentStatus = "Refunded";
                }
            }

            await _dbContext.SaveChangesAsync();

            return (true, $"Hoàn tiền thành công. Số tiền: {payment.Amount:N0} VNĐ");
        }

        public async Task<bool> MarkAsFailedAsync(long paymentId)
        {
            var payment = await _dbContext.Payments.FindAsync(paymentId);
            if (payment == null || payment.Status == "Paid")
                return false;

            payment.Status = "Failed";
            await _dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<bool> IsBookingFullyPaidAsync(long bookingId)
        {
            var booking = await _dbContext.Bookings.FindAsync(bookingId);
            if (booking == null)
                return false;

            var totalPaid = await GetTotalPaidAmountAsync(bookingId);
            var bookingTotal = booking.TotalAmount - booking.DiscountAmount;

            return totalPaid >= bookingTotal;
        }

        public async Task<decimal> GetTotalPaidAmountAsync(long bookingId)
        {
            var totalPaid = await _dbContext.Payments
                .Where(p => p.BookingId == bookingId && p.Status == "Paid")
                .SumAsync(p => p.Amount);

            return totalPaid;
        }

        // Helper method
        private PaymentViewModel MapToViewModel(Payment payment)
        {
            var booking = payment.Booking;
            var bookingRoom = booking?.BookingRooms?.FirstOrDefault();
            var room = bookingRoom?.Room;

            return new PaymentViewModel
            {
                Id = payment.Id,
                BookingId = payment.BookingId,
                BookingCode = $"BK{payment.BookingId:D6}",
                GuestName = booking?.Guest?.FullName ?? "",
                RoomNumber = room?.Number ?? "",
                Method = payment.Method,
                Amount = payment.Amount,
                TxnCode = payment.TxnCode,
                Status = payment.Status,
                CreatedAt = payment.CreatedAt
            };
        }
    }
}
