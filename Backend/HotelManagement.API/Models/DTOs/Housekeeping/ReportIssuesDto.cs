using System.ComponentModel.DataAnnotations;

namespace HotelManagement.API.Models.DTOs.Housekeeping
{
    public class ReportIssuesDto
    {
        [Required]
        public string Issues { get; set; } = string.Empty;
    }
}
