using System.Threading.Tasks;

namespace HotelManagementSystem.Services.Interfaces
{
    public interface IPaymentTransactionService
    {
        /// <summary>
        /// Creates a payment transaction atomically:
        /// 1. Creates a Payment record
        /// 2. Creates an Invoice record
        /// 3. Updates Booking status
        /// </summary>
        Task<(bool Success, string Message, long PaymentId, long InvoiceId)> CreatePaymentTransactionAsync(long bookingId, decimal amount, string method, string? notes = null);

        /// <summary>
        /// Completes a payment (e.g. after successful online payment):
        /// 1. Updates Payment status to Paid
        /// 2. Updates Invoice status to Paid
        /// 3. Updates Booking payment status to Paid
        /// </summary>
        Task<(bool Success, string Message)> CompletePaymentAsync(long paymentId, string txnCode);

        /// <summary>
        /// Refunds a payment:
        /// 1. Updates Payment status to Refunded
        /// 2. Updates Invoice status to Cancelled/Refunded
        /// 3. Updates Booking payment status
        /// </summary>
        Task<(bool Success, string Message)> RefundPaymentAsync(long paymentId, string reason);

        /// <summary>
        /// Gets payment summary for a booking
        /// </summary>
        Task<(decimal TotalAmount, decimal PaidAmount, decimal RemainingAmount)> GetPaymentSummaryAsync(long bookingId);

        /// <summary>
        /// Marks a payment as failed
        /// </summary>
        Task<bool> MarkAsFailedAsync(long paymentId);
    }
}
