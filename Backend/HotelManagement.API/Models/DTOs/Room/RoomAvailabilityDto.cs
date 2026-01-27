namespace HotelManagement.API.Models.DTOs.Room;

public class RoomAvailabilityDto
{
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public long? RoomTypeId { get; set; }
    public long? HotelId { get; set; }
}
