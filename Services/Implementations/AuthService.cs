using System;
using System.Security.Claims;
using System.Threading.Tasks;
using BCrypt.Net;
using HotelManagementSystem.Data;
using HotelManagementSystem.Models.Entities;
using HotelManagementSystem.Models.ViewModels.Auth;
using HotelManagementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

namespace HotelManagementSystem.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly HotelDbContext _context;

        public AuthService(HotelDbContext context)
        {
            _context = context;
        }

        public async Task<User?> AuthenticateAsync(string username, string password)
        {
            // Tìm user theo username
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
            {
                return null;
            }

            // Verify password
            if (!VerifyPassword(password, user.PasswordHash))
            {
                return null;
            }

            return user;
        }

        public async Task<User> RegisterAsync(RegisterViewModel model)
        {
            // Kiểm tra username đã tồn tại
            if (await UsernameExistsAsync(model.Username))
            {
                throw new InvalidOperationException("Tên đăng nhập đã tồn tại");
            }

            // Kiểm tra email đã tồn tại
            if (await EmailExistsAsync(model.Email))
            {
                throw new InvalidOperationException("Email đã được sử dụng");
            }

            // Validate role
            var validRoles = new[] { "Admin", "Manager", "Receptionist", "Housekeeping", "Customer" };
            if (!Array.Exists(validRoles, r => r == model.Role))
            {
                throw new InvalidOperationException("Vai trò không hợp lệ");
            }

            // Tạo user mới
            var user = new User
            {
                Username = model.Username,
                Email = model.Email,
                PasswordHash = HashPassword(model.Password),
                Role = model.Role,
                CreatedAt = DateTime.Now
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Nếu là Customer, tạo luôn record Guest
            if (model.Role == "Customer")
            {
                var guest = new Guest
                {
                    FullName = model.Username, // Tạm lấy username làm tên, user có thể update sau
                    Email = model.Email,
                    CreatedAt = DateTime.Now,
                    UserId = user.Id
                };
                _context.Guests.Add(guest);
                await _context.SaveChangesAsync();
            }

            return user;
        }

        public async Task<bool> UsernameExistsAsync(string username)
        {
            return await _context.Users
                .AnyAsync(u => u.Username == username);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Users
                .AnyAsync(u => u.Email == email);
        }

        public string HashPassword(string password)
        {
            // Sử dụng BCrypt để hash password
            return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
        }

        public bool VerifyPassword(string password, string passwordHash)
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(password, passwordHash);
            }
            catch
            {
                return false;
            }
        }

        public ClaimsIdentity CreateClaimsIdentity(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim(ClaimTypes.Role, user.Role)
            };

            return new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        }

        public async Task<(bool Success, string Message)> UpdateProfileAsync(long userId, string fullName, string email, string phone, string address)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return (false, "Không tìm thấy người dùng");

            // Check email conflict if email changed
            if (user.Email != email && await EmailExistsAsync(email))
            {
                return (false, "Email đã được sử dụng bởi tài khoản khác");
            }

            user.Email = email;

            var guest = await _context.Guests.FirstOrDefaultAsync(g => g.UserId == userId);
            if (guest != null)
            {
                guest.FullName = fullName;
                guest.Email = email;
                guest.Phone = phone;
                // Guest entity currently doesn't have Address, so we ignore it or add it if needed. 
                // Based on SRS, address might be needed, but Guest entity doesn't have it.
                // We will just update what we have.
            }
            else
            {
                // Create guest if missing (should not happen for customers)
                guest = new Guest
                {
                    UserId = userId,
                    FullName = fullName,
                    Email = email,
                    Phone = phone,
                    CreatedAt = DateTime.Now
                };
                _context.Guests.Add(guest);
            }

            await _context.SaveChangesAsync();
            return (true, "Cập nhật thông tin thành công");
        }

        public async Task<(bool Success, string Message)> ChangePasswordAsync(long userId, string currentPassword, string newPassword)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return (false, "Không tìm thấy người dùng");

            if (!VerifyPassword(currentPassword, user.PasswordHash))
            {
                return (false, "Mật khẩu hiện tại không đúng");
            }

            user.PasswordHash = HashPassword(newPassword);
            await _context.SaveChangesAsync();
            return (true, "Đổi mật khẩu thành công");
        }
    }
}
