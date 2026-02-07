using System.ComponentModel.DataAnnotations;

namespace HotelManagement.API.Models.DTOs.Promotion
{
    public class CreatePromotionDto
    {
        [Required]
        [StringLength(32)]
        public string Code { get; set; } = string.Empty;

        [Required]
        public string Type { get; set; } = string.Empty; // Percent or Amount

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Value { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public string? ConditionsJson { get; set; }
    }
}
