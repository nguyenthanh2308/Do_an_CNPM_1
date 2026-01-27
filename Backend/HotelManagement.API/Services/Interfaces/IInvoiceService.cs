using HotelManagement.API.Models.DTOs.Payment;

namespace HotelManagement.API.Services.Interfaces;

public interface IInvoiceService
{
    Task<InvoiceDto> GenerateInvoiceAsync(long bookingId);
    Task<InvoiceDto> GetInvoiceByBookingIdAsync(long bookingId);
    Task<InvoiceDto> GetInvoiceByIdAsync(long id);
    Task<InvoiceDto> GetInvoiceByInvoiceNumberAsync(string invoiceNumber);
}
