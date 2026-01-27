namespace HotelManagement.API.Models.DTOs.Hotel;

public class CreateHotelDto
{
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string Timezone { get; set; } = "Asia/Ho_Chi_Minh";
}
