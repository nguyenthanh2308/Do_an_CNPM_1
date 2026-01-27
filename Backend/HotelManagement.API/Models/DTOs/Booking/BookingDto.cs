namespace HotelManagement.API.Models.DTOs.Booking;

public class BookingDto
{
    public long Id { get; set; }
    public long HotelId { get; set; }
    public string HotelName { get; set; } = string.Empty;
    public long GuestId { get; set; }
    public string GuestName { get; set; } = string.Empty;
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string PaymentStatus { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
