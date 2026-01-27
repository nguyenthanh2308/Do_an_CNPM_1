namespace HotelManagement.API.Models.DTOs.RoomType;

public class UpdateRoomTypeDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public byte Capacity { get; set; }
    public decimal BasePrice { get; set; }
    public List<long>? AmenityIds { get; set; }
}
