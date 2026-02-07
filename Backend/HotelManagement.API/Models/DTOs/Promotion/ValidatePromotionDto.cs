using System.ComponentModel.DataAnnotations;

namespace HotelManagement.API.Models.DTOs.Promotion
{
    public class ValidatePromotionDto
    {
        [Required]
        public string Code { get; set; } = string.Empty;

        [Required]
        [Range(0, double.MaxValue)]
        public decimal BookingAmount { get; set; }
    }
}
