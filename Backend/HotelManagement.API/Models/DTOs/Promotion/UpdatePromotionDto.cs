using System.ComponentModel.DataAnnotations;

namespace HotelManagement.API.Models.DTOs.Promotion
{
    public class UpdatePromotionDto
    {
        [StringLength(32)]
        public string? Code { get; set; }

        public string? Type { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? Value { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public string? ConditionsJson { get; set; }
    }
}
