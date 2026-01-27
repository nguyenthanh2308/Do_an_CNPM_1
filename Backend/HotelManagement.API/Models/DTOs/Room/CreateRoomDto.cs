namespace HotelManagement.API.Models.DTOs.Room;

public class CreateRoomDto
{
    public string RoomNumber { get; set; } = string.Empty;
    public long RoomTypeId { get; set; }
    public long HotelId { get; set; }
    public decimal BasePrice { get; set; }
    public int Floor { get; set; }
    public string? ViewType { get; set; }
    public string Status { get; set; } = "Available";
}
