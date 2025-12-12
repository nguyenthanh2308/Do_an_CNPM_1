using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace HotelManagementSystem.Models.ViewModels.RoomVM
{
    public class RoomViewModel
    {
        public long Id { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn khách sạn")]
        [Display(Name = "Khách sạn")]
        public long HotelId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn loại phòng")]
        [Display(Name = "Loại phòng")]
        public long RoomTypeId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số phòng")]
        [MaxLength(16)]
        [Display(Name = "Số phòng")]
        public string Number { get; set; } = null!;

        [Display(Name = "Tầng")]
        public short? Floor { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn trạng thái")]
        [Display(Name = "Trạng thái")]
        public string Status { get; set; } = "Vacant";

        [Display(Name = "URL hình ảnh")]
        [MaxLength(255)]
        public string? ImageUrl { get; set; }

        [Display(Name = "Hình ảnh phòng")]
        public IFormFile? ImageFile { get; set; }

        public DateTime CreatedAt { get; set; }

        // For display
        public string? HotelName { get; set; }
        public string? RoomTypeName { get; set; }
    }
}
