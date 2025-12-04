using System.ComponentModel.DataAnnotations;

namespace HotelManagementSystem.Models.ViewModels.Amenity
{
    public class AmenityViewModel
    {
        public long Id { get; set; }

        [Required(ErrorMessage = "Tên tiện nghi không được để trống")]
        [StringLength(64, ErrorMessage = "Tên tiện nghi không được vượt quá 64 ký tự")]
        [Display(Name = "Tên tiện nghi")]
        public string Name { get; set; } = null!;

        [Display(Name = "Số loại phòng sử dụng")]
        public int UsageCount { get; set; }
    }
}
