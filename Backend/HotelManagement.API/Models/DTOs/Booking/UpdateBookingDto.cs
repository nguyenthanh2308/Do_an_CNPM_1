namespace HotelManagement.API.Models.DTOs.Booking;

public class UpdateBookingDto
{
    public DateTime? CheckInDate { get; set; }
    public DateTime? CheckOutDate { get; set; }
    public string? Status { get; set; }
    public string? SpecialRequests { get; set; }
}
