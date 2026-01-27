namespace HotelManagement.API.Models.DTOs.Room;

public class UpdateRoomDto
{
    public string? RoomNumber { get; set; }
    public long? RoomTypeId { get; set; }
    public short? Floor { get; set; }
    public string? ViewType { get; set; }
    public string? Status { get; set; }
}
