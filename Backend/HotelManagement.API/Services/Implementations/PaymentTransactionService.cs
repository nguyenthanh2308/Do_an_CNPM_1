using HotelManagement.API.Exceptions;
using HotelManagement.API.Models.DTOs.Common;
using HotelManagement.API.Models.Entities;
using HotelManagement.API.Repositories.Interfaces;
using HotelManagement.API.Services.Interfaces;

namespace HotelManagement.API.Services.Implementations
{
    public class PaymentTransactionService : IPaymentTransactionService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PaymentTransactionService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Payment>> GetAllTransactionsAsync()
        {
            return await _unitOfWork.Payments.GetAllAsync();
        }

        public async Task<Payment?> GetTransactionByIdAsync(long id)
        {
            return await _unitOfWork.Payments.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Payment>> GetTransactionsByBookingAsync(long bookingId)
        {
            return await _unitOfWork.Payments.GetPaymentsByBookingAsync(bookingId);
        }

        public async Task<decimal> GetTotalPaidForBookingAsync(long bookingId)
        {
            return await _unitOfWork.Payments.GetTotalPaidAsync(bookingId);
        }

        public async Task<Payment> CreateTransactionAsync(Payment payment)
        {
            // Validate booking exists
            var booking = await _unitOfWork.Bookings.GetByIdAsync(payment.BookingId);
            if (booking == null)
                throw new NotFoundException("Booking", payment.BookingId);

            // Validate amount
            if (payment.Amount <= 0)
                throw new ValidationException("Payment amount must be greater than zero.");

            // Check if total payments exceed booking amount
            var totalPaid = await _unitOfWork.Payments.GetTotalPaidAsync(payment.BookingId);
            if (totalPaid + payment.Amount > booking.TotalAmount)
                throw new BusinessException("Total payments cannot exceed booking amount.");

            payment.CreatedAt = DateTime.Now;
            payment.Status = payment.Status ?? "Unpaid";

            await _unitOfWork.Payments.AddAsync(payment);
            await _unitOfWork.SaveChangesAsync();

            // Update booking payment status if fully paid
            if (totalPaid + payment.Amount >= booking.TotalAmount && payment.Status == "Paid")
            {
                booking.PaymentStatus = "Paid";
                await _unitOfWork.Bookings.UpdateAsync(booking);
                await _unitOfWork.SaveChangesAsync();
            }

            return payment;
        }

        public async Task<Payment> UpdateTransactionStatusAsync(long id, string status)
        {
            var payment = await _unitOfWork.Payments.GetByIdAsync(id);
            if (payment == null)
                throw new NotFoundException("Payment", id);

            var validStatuses = new[] { "Unpaid", "Paid", "Refunded", "Failed" };
            if (!validStatuses.Contains(status))
                throw new ValidationException($"Invalid status. Must be one of: {string.Join(", ", validStatuses)}");

            payment.Status = status;

            await _unitOfWork.Payments.UpdateAsync(payment);
            await _unitOfWork.SaveChangesAsync();

            // Update booking payment status
            var booking = await _unitOfWork.Bookings.GetByIdAsync(payment.BookingId);
            if (booking != null)
            {
                var totalPaid = await _unitOfWork.Payments.GetTotalPaidAsync(payment.BookingId);
                
                if (totalPaid >= booking.TotalAmount)
                    booking.PaymentStatus = "Paid";
                else if (totalPaid > 0)
                    booking.PaymentStatus = "Partial";
                else
                    booking.PaymentStatus = "Unpaid";

                await _unitOfWork.Bookings.UpdateAsync(booking);
                await _unitOfWork.SaveChangesAsync();
            }

            return payment;
        }

        public async Task<bool> ProcessRefundAsync(long paymentId, decimal refundAmount)
        {
            var payment = await _unitOfWork.Payments.GetByIdAsync(paymentId);
            if (payment == null)
                throw new NotFoundException("Payment", paymentId);

            if (payment.Status != "Paid")
                throw new BusinessException("Can only refund paid transactions.");

            if (refundAmount <= 0 || refundAmount > payment.Amount)
                throw new ValidationException("Invalid refund amount.");

            // Create refund transaction
            var refund = new Payment
            {
                BookingId = payment.BookingId,
                Amount = -refundAmount,
                Method = payment.Method,
                Status = "Refunded",
                TxnCode = $"REFUND-{payment.TxnCode}",
                CreatedAt = DateTime.Now
            };

            await _unitOfWork.Payments.AddAsync(refund);

            // Update original payment status
            payment.Status = "Refunded";
            await _unitOfWork.Payments.UpdateAsync(payment);

            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<PagedResult<Payment>> GetPagedTransactionsAsync(int pageNumber, int pageSize, long? bookingId = null, string? status = null)
        {
            var (items, totalCount) = await _unitOfWork.Payments.GetPagedAsync(
                pageNumber,
                pageSize,
                filter: p => 
                    (!bookingId.HasValue || p.BookingId == bookingId.Value) &&
                    (string.IsNullOrEmpty(status) || p.Status == status),
                orderBy: query => query.OrderByDescending(p => p.CreatedAt)
            );

            return new PagedResult<Payment>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
    }
}
