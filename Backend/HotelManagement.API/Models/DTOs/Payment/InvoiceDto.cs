namespace HotelManagement.API.Models.DTOs.Payment;

public class InvoiceDto
{
    public long Id { get; set; }
    public long BookingId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public decimal Subtotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Notes { get; set; }
    public DateTime IssuedAt { get; set; }
}
