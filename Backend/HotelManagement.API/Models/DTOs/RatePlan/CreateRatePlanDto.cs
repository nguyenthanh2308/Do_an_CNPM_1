using System.ComponentModel.DataAnnotations;

namespace HotelManagement.API.Models.DTOs.RatePlan
{
    public class CreateRatePlanDto
    {
        [Required]
        public long RoomTypeId { get; set; }

        [Required]
        [StringLength(128)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Type { get; set; } = string.Empty; // Flexible or NonRefundable

        public int? FreeCancelUntilHours { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        public string? WeekendRuleJson { get; set; }
    }
}
