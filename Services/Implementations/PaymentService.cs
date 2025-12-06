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

        // Methods moved to PaymentTransactionService
        // CreatePaymentAsync
        // ProcessMockPaymentAsync
        // ProcessPayAtPropertyAsync
        // ProcessRefundAsync
        // MarkAsFailedAsync

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
