namespace HotelManagement.API.Models.DTOs.Guest;

public class CreateGuestDto
{
    public long? UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? IdNumber { get; set; }
}
