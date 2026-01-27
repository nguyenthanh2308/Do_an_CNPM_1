using HotelManagement.API.Models.DTOs.Common;
using HotelManagement.API.Models.Entities;

namespace HotelManagement.API.Services.Interfaces
{
    public interface IPaymentTransactionService
    {
        Task<IEnumerable<Payment>> GetAllTransactionsAsync();
        Task<Payment?> GetTransactionByIdAsync(long id);
        Task<IEnumerable<Payment>> GetTransactionsByBookingAsync(long bookingId);
        Task<decimal> GetTotalPaidForBookingAsync(long bookingId);
        Task<Payment> CreateTransactionAsync(Payment payment);
        Task<Payment> UpdateTransactionStatusAsync(long id, string status);
        Task<bool> ProcessRefundAsync(long paymentId, decimal refundAmount);
        Task<PagedResult<Payment>> GetPagedTransactionsAsync(int pageNumber, int pageSize, long? bookingId = null, string? status = null);
    }
}
