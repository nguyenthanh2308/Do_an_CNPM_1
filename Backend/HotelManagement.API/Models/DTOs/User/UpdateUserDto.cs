using System.ComponentModel.DataAnnotations;

namespace HotelManagement.API.Models.DTOs.User
{
    public class UpdateUserDto
    {
        [EmailAddress]
        public string? Email { get; set; }

        public string? Role { get; set; }
    }
}
