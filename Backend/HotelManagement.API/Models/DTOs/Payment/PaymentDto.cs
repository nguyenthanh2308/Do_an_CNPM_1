namespace HotelManagement.API.Models.DTOs.Payment;

public class PaymentDto
{
    public long Id { get; set; }
    public long BookingId { get; set; }
    public string Method { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? TxnCode { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
