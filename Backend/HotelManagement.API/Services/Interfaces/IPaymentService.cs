using HotelManagement.API.Models.DTOs.Payment;

namespace HotelManagement.API.Services.Interfaces;

public interface IPaymentService
{
    Task<PaymentDto> ProcessPaymentAsync(CreatePaymentDto dto);
    Task<IEnumerable<PaymentDto>> GetPaymentsByBookingAsync(long bookingId);
    Task<PaymentDto> GetPaymentByIdAsync(long id);
    Task<decimal> GetTotalPaidForBookingAsync(long bookingId);
    Task<PaymentDto> RefundPaymentAsync(long paymentId, string reason);
}
