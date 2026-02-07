using System.ComponentModel.DataAnnotations;

namespace HotelManagement.API.Models.DTOs.Housekeeping
{
    public class UpdateTaskStatusDto
    {
        [Required]
        public string Status { get; set; } = string.Empty;
    }
}
