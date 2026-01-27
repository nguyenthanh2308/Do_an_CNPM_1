namespace HotelManagement.API.Models.DTOs.Hotel;

public class HotelDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Address { get; set; } = string.Empty;
    public string? City { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public int? StarRating { get; set; }
}
