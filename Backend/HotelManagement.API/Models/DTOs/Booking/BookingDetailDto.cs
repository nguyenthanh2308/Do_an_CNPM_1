using HotelManagement.API.Models.DTOs.Room;
using HotelManagement.API.Models.DTOs.Guest;
using HotelManagement.API.Models.DTOs.Payment;
using HotelManagement.API.Models.DTOs.Hotel;

namespace HotelManagement.API.Models.DTOs.Booking;

public class BookingDetailDto
{
    public long Id { get; set; }
    public HotelDto Hotel { get; set; } = null!;
    public GuestDto Guest { get; set; } = null!;
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string PaymentStatus { get; set; } = string.Empty;
    public List<RoomDto> Rooms { get; set; } = new();
    public List<PaymentDto> Payments { get; set; } = new();
    public InvoiceDto? Invoice { get; set; }
    public string? SpecialRequests { get; set; }
    public DateTime CreatedAt { get; set; }
}
