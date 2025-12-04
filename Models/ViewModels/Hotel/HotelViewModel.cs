using System;
using System.ComponentModel.DataAnnotations;

namespace HotelManagementSystem.Models.ViewModels.Hotel
{
    public class HotelViewModel
    {
        public long Id { get; set; }

        [Required(ErrorMessage = "Tên khách sạn không được để trống")]
        [StringLength(128, ErrorMessage = "Tên khách sạn không được vượt quá 128 ký tự")]
        [Display(Name = "Tên khách sạn")]
        public string Name { get; set; } = null!;

        [StringLength(255, ErrorMessage = "Địa chỉ không được vượt quá 255 ký tự")]
        [Display(Name = "Địa chỉ")]
        public string? Address { get; set; }

        [Required(ErrorMessage = "Múi giờ không được để trống")]
        [StringLength(64, ErrorMessage = "Múi giờ không được vượt quá 64 ký tự")]
        [Display(Name = "Múi giờ")]
        public string Timezone { get; set; } = "Asia/Ho_Chi_Minh";

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedAt { get; set; }

        // Thống kê
        [Display(Name = "Số loại phòng")]
        public int RoomTypeCount { get; set; }

        [Display(Name = "Tổng số phòng")]
        public int TotalRooms { get; set; }

        [Display(Name = "Số booking")]
        public int TotalBookings { get; set; }
    }
}
