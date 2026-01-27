namespace HotelManagement.API.Models.DTOs.Room;

public class RoomDto
{
    public long Id { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public long RoomTypeId { get; set; }
    public string RoomTypeName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
    public int Floor { get; set; }
    public string? ViewType { get; set; }
    public long HotelId { get; set; }
    public string HotelName { get; set; } = string.Empty;
}
