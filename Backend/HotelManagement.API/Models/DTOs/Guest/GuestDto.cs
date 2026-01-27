namespace HotelManagement.API.Models.DTOs.Guest;

public class GuestDto
{
    public long Id { get; set; }
    public long? UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? IdNumber { get; set; }
    public DateTime CreatedAt { get; set; }
}
