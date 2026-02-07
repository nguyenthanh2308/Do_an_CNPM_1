using System.ComponentModel.DataAnnotations;

namespace HotelManagement.API.Models.DTOs.Housekeeping
{
    public class AddTaskNotesDto
    {
        [Required]
        public string Notes { get; set; } = string.Empty;
    }
}
