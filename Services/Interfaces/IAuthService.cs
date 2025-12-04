using System.Security.Claims;
using System.Threading.Tasks;
using HotelManagementSystem.Models.Entities;
using HotelManagementSystem.Models.ViewModels.Auth;

namespace HotelManagementSystem.Services.Interfaces
{
    public interface IAuthService
    {
        /// <summary>
        /// Xác thực user với username và password
        /// </summary>
        /// <returns>User nếu thành công, null nếu thất bại</returns>
        Task<User?> AuthenticateAsync(string username, string password);

        /// <summary>
        /// Đăng ký user mới
        /// </summary>
        /// <returns>User được tạo</returns>
        Task<User> RegisterAsync(RegisterViewModel model);

        /// <summary>
        /// Kiểm tra xem username đã tồn tại chưa
        /// </summary>
        Task<bool> UsernameExistsAsync(string username);

        /// <summary>
        /// Kiểm tra xem email đã tồn tại chưa
        /// </summary>
        Task<bool> EmailExistsAsync(string email);

        /// <summary>
        /// Hash password sử dụng BCrypt
        /// </summary>
        string HashPassword(string password);

        /// <summary>
        /// Verify password với hash
        /// </summary>
        bool VerifyPassword(string password, string passwordHash);

        /// <summary>
        /// Tạo claims cho user để đăng nhập
        /// </summary>
        ClaimsIdentity CreateClaimsIdentity(User user);

        /// <summary>
        /// Cập nhật thông tin profile (User + Guest)
        /// </summary>
        Task<(bool Success, string Message)> UpdateProfileAsync(long userId, string fullName, string email, string phone, string address);

        /// <summary>
        /// Đổi mật khẩu
        /// </summary>
        Task<(bool Success, string Message)> ChangePasswordAsync(long userId, string currentPassword, string newPassword);
    }
}
