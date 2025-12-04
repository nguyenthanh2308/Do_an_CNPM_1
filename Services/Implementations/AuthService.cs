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
    }
}
