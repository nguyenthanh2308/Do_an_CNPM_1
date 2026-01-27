namespace HotelManagement.API.Models.DTOs.Guest;

public class UpdateGuestDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? IdentityNumber { get; set; }
    public string? Address { get; set; }
    public DateTime? DateOfBirth { get; set; }
}
