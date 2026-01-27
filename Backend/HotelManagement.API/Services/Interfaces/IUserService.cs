using HotelManagement.API.Models.Entities;

namespace HotelManagement.API.Services.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<User?> GetUserByIdAsync(long id);
        Task<User?> GetUserByUsernameAsync(string username);
        Task<IEnumerable<User>> GetUsersByRoleAsync(string role);
        Task<User> CreateUserAsync(User user, string password);
        Task<User> UpdateUserAsync(long id, User user);
        Task<bool> DeactivateUserAsync(long id);
        Task<bool> DeleteUserAsync(long id);
    }
}
