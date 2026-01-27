namespace HotelManagement.API.Models.DTOs.RoomType;

public class RoomTypeDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public long HotelId { get; set; }
    public string? HotelName { get; set; }
    public byte Capacity { get; set; }
    public decimal BasePrice { get; set; }
    public IEnumerable<string>? Amenities { get; set; }
}
