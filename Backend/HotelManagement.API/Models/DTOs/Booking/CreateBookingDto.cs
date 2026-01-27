namespace HotelManagement.API.Models.DTOs.Booking;

public class CreateBookingDto
{
    public long HotelId { get; set; }
    public long GuestId { get; set; }
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public List<long> RoomIds { get; set; } = new();
    public long? PromotionId { get; set; }
    public string? SpecialRequests { get; set; }
}
