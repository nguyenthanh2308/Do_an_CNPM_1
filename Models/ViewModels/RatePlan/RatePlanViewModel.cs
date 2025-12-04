using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HotelManagementSystem.Models.ViewModels.RatePlan
{
    public class RatePlanViewModel
    {
        public long Id { get; set; }

        [Required(ErrorMessage = "Loại phòng không được để trống")]
        [Display(Name = "Loại phòng")]
        public long RoomTypeId { get; set; }

        [Required(ErrorMessage = "Tên rate plan không được để trống")]
        [StringLength(128, ErrorMessage = "Tên không được vượt quá 128 ký tự")]
        [Display(Name = "Tên rate plan")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Loại rate plan không được để trống")]
        [Display(Name = "Loại rate plan")]
        public string Type { get; set; } = "Flexible";

        [Display(Name = "Hủy miễn phí trước (giờ)")]
        [Range(0, 168, ErrorMessage = "Giờ hủy miễn phí phải từ 0 đến 168 (7 ngày)")]
        public int? FreeCancelUntilHours { get; set; }

        [Required(ErrorMessage = "Ngày bắt đầu không được để trống")]
        [Display(Name = "Ngày bắt đầu")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Ngày kết thúc không được để trống")]
        [Display(Name = "Ngày kết thúc")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [Required(ErrorMessage = "Giá không được để trống")]
        [Range(0, 999999999.99, ErrorMessage = "Giá phải từ 0 đến 999,999,999.99")]
        [Display(Name = "Giá (VNĐ/đêm)")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal Price { get; set; }

        [Display(Name = "Áp dụng giá cuối tuần")]
        public bool IsWeekendRateActive { get; set; }

        [Display(Name = "Điều chỉnh giá cuối tuần (%)")]
        [Range(-100, 1000, ErrorMessage = "Điều chỉnh giá cuối tuần phải từ -100% đến 1000%")]
        public decimal? WeekendAdjustmentPercent { get; set; }

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        [Display(Name = "Tên loại phòng")]
        public string? RoomTypeName { get; set; }

        [Display(Name = "Khách sạn")]
        public string? HotelName { get; set; }

        [Display(Name = "Giá cơ bản loại phòng")]
        public decimal? BasePrice { get; set; }

        // For dropdown selections
        public SelectList? RoomTypes { get; set; }

        // Display helpers
        public string TypeDisplay
        {
            get
            {
                return Type switch
                {
                    "Flexible" => "Linh hoạt (Flexible)",
                    "NonRefundable" => "Không hoàn tiền (Non-Refundable)",
                    _ => Type
                };
            }
        }

        public decimal CalculateFinalPrice(bool isWeekend = false)
        {
            var finalPrice = Price;

            // Apply weekend adjustment if applicable
            if (isWeekend && IsWeekendRateActive && WeekendAdjustmentPercent.HasValue)
            {
                finalPrice = finalPrice * (1 + WeekendAdjustmentPercent.Value / 100);
            }

            return Math.Round(finalPrice, 2);
        }

        public bool IsCurrentlyEffective
        {
            get
            {
                var now = DateTime.Now.Date;
                return now >= StartDate.Date && now <= EndDate.Date;
            }
        }

        public int DaysActive
        {
            get
            {
                return (EndDate.Date - StartDate.Date).Days + 1;
            }
        }
    }
}
