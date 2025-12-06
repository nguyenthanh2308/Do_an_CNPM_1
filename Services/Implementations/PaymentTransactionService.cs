using System;
using System.Linq;
using System.Threading.Tasks;
using HotelManagementSystem.Data;
using HotelManagementSystem.Models.Entities;
using HotelManagementSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelManagementSystem.Services.Implementations
{
    public class PaymentTransactionService : IPaymentTransactionService
    {
        private readonly HotelDbContext _context;
        private readonly IInvoiceService _invoiceService;

        public PaymentTransactionService(HotelDbContext context, IInvoiceService invoiceService)
        {
            _context = context;
            _invoiceService = invoiceService;
        }

        public async Task<(bool Success, string Message, long PaymentId, long InvoiceId)> CreatePaymentTransactionAsync(long bookingId, decimal amount, string method, string? notes = null)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var booking = await _context.Bookings.FindAsync(bookingId);
                if (booking == null)
                {
                    return (false, "Booking not found", 0, 0);
                }

                // 1. Create Payment Record
                var payment = new Payment
                {
                    BookingId = bookingId,
                    Amount = amount,
                    Method = method,
                    Status = "Pending", // Default to Pending, will be updated to Paid if online payment succeeds immediately
                    TxnCode = "PENDING-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                    CreatedAt = DateTime.Now
                };
                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();

                // 2. Create Invoice (if not exists)
                // Note: InvoiceService handles checking if invoice exists
                var invoiceViewModel = await _invoiceService.CreateInvoiceAsync(bookingId, notes);
                long invoiceId = 0;
                
                if (invoiceViewModel != null)
                {
                    invoiceId = (long)invoiceViewModel.Id;
                }
                else
                {
                    // If invoice already exists, get it
                    var existingInvoice = await _context.Invoices.FirstOrDefaultAsync(i => i.BookingId == bookingId);
                    if (existingInvoice != null)
                    {
                        invoiceId = existingInvoice.Id;
                    }
                }

                // 3. Update Booking Status (if needed)
                // If it's a "PayAtProperty" method, we might confirm the booking immediately
                if (method == "PayAtProperty")
                {
                    booking.Status = "Confirmed";
                    // Payment remains Pending until actual payment at property
                    // Invoice remains Unpaid
                }
                
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return (true, "Transaction created successfully", payment.Id, invoiceId);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, "Error creating transaction: " + ex.Message, 0, 0);
            }
        }

        public async Task<(bool Success, string Message)> CompletePaymentAsync(long paymentId, string txnCode)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var payment = await _context.Payments.FindAsync(paymentId);
                if (payment == null) return (false, "Payment not found");

                var booking = await _context.Bookings.FindAsync(payment.BookingId);
                if (booking == null) return (false, "Booking not found");

                // 1. Update Payment
                payment.Status = "Paid";
                payment.TxnCode = txnCode;
                // payment.PaidAt = DateTime.Now; // Assuming Payment entity has PaidAt, if not, rely on Status

                // 2. Update Invoice
                var invoice = await _context.Invoices.FirstOrDefaultAsync(i => i.BookingId == payment.BookingId);
                if (invoice != null)
                {
                    invoice.Status = "Paid";
                    invoice.PaidAt = DateTime.Now;
                    invoice.PaymentMethod = payment.Method;
                }

                // 3. Update Booking
                booking.PaymentStatus = "Paid";
                booking.Status = "Confirmed";

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return (true, "Payment completed successfully");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, "Error completing payment: " + ex.Message);
            }
        }

        public async Task<(bool Success, string Message)> RefundPaymentAsync(long paymentId, string reason)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var payment = await _context.Payments.FindAsync(paymentId);
                if (payment == null) return (false, "Payment not found");

                if (payment.Status != "Paid") return (false, "Cannot refund unpaid payment");

                // 1. Update Payment
                payment.Status = "Refunded";
                
                // 2. Update Invoice
                var invoice = await _context.Invoices.FirstOrDefaultAsync(i => i.BookingId == payment.BookingId);
                if (invoice != null)
                {
                    invoice.Status = "Cancelled"; // Or Refunded if supported
                    invoice.Notes += $"\nRefunded: {reason}";
                }

                // 3. Update Booking
                var booking = await _context.Bookings.FindAsync(payment.BookingId);
                if (booking != null)
                {
                    booking.PaymentStatus = "Refunded";
                    booking.Status = "Cancelled";
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return (true, "Payment refunded successfully");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, "Error refunding payment: " + ex.Message);
            }
        }

        public async Task<(decimal TotalAmount, decimal PaidAmount, decimal RemainingAmount)> GetPaymentSummaryAsync(long bookingId)
        {
            var booking = await _context.Bookings.FindAsync(bookingId);
            if (booking == null) return (0, 0, 0);

            var totalAmount = booking.TotalAmount - booking.DiscountAmount;
            
            // Calculate tax and service charge (logic duplicated from InvoiceService - should be centralized)
            var taxAmount = Math.Round(totalAmount * 0.1m, 0);
            var serviceCharge = Math.Round(totalAmount * 0.05m, 0);
            var finalTotal = totalAmount + taxAmount + serviceCharge;

            var paidAmount = await _context.Payments
                .Where(p => p.BookingId == bookingId && p.Status == "Paid")
                .SumAsync(p => p.Amount);

            return (finalTotal, paidAmount, finalTotal - paidAmount);
        }

        public async Task<bool> MarkAsFailedAsync(long paymentId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var payment = await _context.Payments.FindAsync(paymentId);
                if (payment == null || payment.Status == "Paid")
                    return false;

                payment.Status = "Failed";
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }
    }
}
