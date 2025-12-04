using System;
using System.ComponentModel.DataAnnotations;

namespace HotelManagementSystem.Models.ViewModels.Guest
{
    public class GuestViewModel
    {
        public long Id { get; set; }

        [Required(ErrorMessage = "Họ và tên không được để trống")]
        [StringLength(128, ErrorMessage = "Họ và tên không được vượt quá 128 ký tự")]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; } = null!;

        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(128, ErrorMessage = "Email không được vượt quá 128 ký tự")]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [StringLength(32, ErrorMessage = "Số điện thoại không được vượt quá 32 ký tự")]
        [Display(Name = "Số điện thoại")]
        public string? Phone { get; set; }

        [StringLength(32, ErrorMessage = "Số CMND/CCCD không được vượt quá 32 ký tự")]
        [Display(Name = "Số CMND/CCCD")]
        public string? IdNumber { get; set; }

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedAt { get; set; }

        // Statistics
        [Display(Name = "Tổng số booking")]
        public int TotalBookings { get; set; }

        [Display(Name = "Booking hoàn thành")]
        public int CompletedBookings { get; set; }

        [Display(Name = "Booking đã hủy")]
        public int CancelledBookings { get; set; }

        [Display(Name = "Tổng chi tiêu")]
        public decimal TotalSpent { get; set; }

        [Display(Name = "Booking gần nhất")]
        public DateTime? LastBookingDate { get; set; }

        // Helper properties
        public bool HasEmail => !string.IsNullOrWhiteSpace(Email);
        
        public bool HasPhone => !string.IsNullOrWhiteSpace(Phone);
        
        public bool HasIdNumber => !string.IsNullOrWhiteSpace(IdNumber);

        public bool IsNewCustomer => TotalBookings == 0;

        public string CustomerTypeDisplay
        {
            get
            {
                if (TotalBookings == 0) return "Khách mới";
                if (TotalBookings >= 10) return "Khách VIP";
                if (TotalBookings >= 5) return "Khách thân thiết";
                return "Khách thường";
            }
        }

        public string CustomerTypeBadgeClass
        {
            get
            {
                if (TotalBookings == 0) return "bg-info";
                if (TotalBookings >= 10) return "bg-warning";
                if (TotalBookings >= 5) return "bg-success";
                return "bg-secondary";
            }
        }

        public string ContactInfo
        {
            get
            {
                var parts = new System.Collections.Generic.List<string>();
                if (HasEmail) parts.Add($"📧 {Email}");
                if (HasPhone) parts.Add($"📱 {Phone}");
                if (!parts.Any()) return "Chưa có thông tin liên hệ";
                return string.Join(" | ", parts);
            }
        }

        public int DaysSinceCreated => (DateTime.Now - CreatedAt).Days;

        public bool HasRecentActivity
        {
            get
            {
                if (!LastBookingDate.HasValue) return false;
                return (DateTime.Now - LastBookingDate.Value).Days <= 90;
            }
        }
    }
}
