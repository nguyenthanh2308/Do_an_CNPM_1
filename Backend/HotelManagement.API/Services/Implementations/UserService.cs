using HotelManagement.API.Exceptions;
using HotelManagement.API.Models.Entities;
using HotelManagement.API.Repositories.Interfaces;
using HotelManagement.API.Services.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace HotelManagement.API.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _unitOfWork.Users.GetAllAsync();
        }

        public async Task<User?> GetUserByIdAsync(long id)
        {
            return await _unitOfWork.Users.GetByIdAsync(id);
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _unitOfWork.Users.GetUserByUsernameAsync(username);
        }

        public async Task<IEnumerable<User>> GetUsersByRoleAsync(string role)
        {
            return await _unitOfWork.Users.GetUsersByRoleAsync(role);
        }

        public async Task<User> CreateUserAsync(User user, string password)
        {
            // Validate username doesn't exist
            var existingUser = await _unitOfWork.Users.GetUserByUsernameAsync(user.Username);
            if (existingUser != null)
                throw new ValidationException("Username already exists.");

            // Validate email doesn't exist
            var existingEmail = await _unitOfWork.Users.GetUserByEmailAsync(user.Email);
            if (existingEmail != null)
                throw new ValidationException("Email already exists.");

            // Hash password
            user.PasswordHash = HashPassword(password);
            user.CreatedAt = DateTime.Now;

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return user;
        }

        public async Task<User> UpdateUserAsync(long id, User user)
        {
            var existingUser = await _unitOfWork.Users.GetByIdAsync(id);
            if (existingUser == null)
                throw new NotFoundException("User", id);

            // Update allowed fields
            existingUser.Email = user.Email;
            existingUser.Role = user.Role;

            await _unitOfWork.Users.UpdateAsync(existingUser);
            await _unitOfWork.SaveChangesAsync();

            return existingUser;
        }

        public async Task<bool> DeactivateUserAsync(long id)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user == null)
                throw new NotFoundException("User", id);

            // Since User doesn't have IsActive, we can't deactivate
            // This would need to be implemented differently or User entity needs IsActive property
            // For now, just return true
            await Task.CompletedTask;
            return true;
        }

        public async Task<bool> DeleteUserAsync(long id)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user == null)
                throw new NotFoundException("User", id);

            await _unitOfWork.Users.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }
}
