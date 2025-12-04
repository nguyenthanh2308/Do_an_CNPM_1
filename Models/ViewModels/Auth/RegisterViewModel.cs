using System.ComponentModel.DataAnnotations;

namespace HotelManagementSystem.Models.ViewModels.Auth
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
        [StringLength(64, ErrorMessage = "Tên đăng nhập không được vượt quá 64 ký tự")]
        [Display(Name = "Tên đăng nhập")]
        public string Username { get; set; } = null!;

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(128, ErrorMessage = "Email không được vượt quá 128 ký tự")]
        [Display(Name = "Email")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "Xác nhận mật khẩu không được để trống")]
        [DataType(DataType.Password)]
        [Display(Name = "Xác nhận mật khẩu")]
        [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        public string ConfirmPassword { get; set; } = null!;

        // Role sẽ tự động được set, không cần user nhập
        public string Role { get; set; } = "Housekeeping";
    }
}
