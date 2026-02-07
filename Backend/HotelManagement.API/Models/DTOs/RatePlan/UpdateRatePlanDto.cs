using System.ComponentModel.DataAnnotations;

namespace HotelManagement.API.Models.DTOs.RatePlan
{
    public class UpdateRatePlanDto
    {
        [StringLength(128)]
        public string? Name { get; set; }

        public string? Type { get; set; }

        public int? FreeCancelUntilHours { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? Price { get; set; }

        public string? WeekendRuleJson { get; set; }
    }
}
