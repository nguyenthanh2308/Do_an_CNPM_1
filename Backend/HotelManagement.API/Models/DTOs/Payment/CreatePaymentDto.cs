namespace HotelManagement.API.Models.DTOs.Payment;

public class CreatePaymentDto
{
    public long BookingId { get; set; }
    public string Method { get; set; } = string.Empty; // Cash, Card, Transfer, VNPAY
    public decimal Amount { get; set; }
    public string? TxnCode { get; set; }
}
