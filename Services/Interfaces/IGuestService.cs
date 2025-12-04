using System.Collections.Generic;
using System.Threading.Tasks;
using HotelManagementSystem.Models.ViewModels.Guest;

namespace HotelManagementSystem.Services.Interfaces
{
    public interface IGuestService
    {
        Task<List<GuestViewModel>> GetAllAsync();
        Task<GuestViewModel?> GetByIdAsync(long id);
        Task<List<GuestViewModel>> SearchAsync(string? keyword);
        Task<bool> CreateAsync(GuestViewModel model);
        Task<bool> UpdateAsync(GuestViewModel model);
        Task<bool> DeleteAsync(long id);
        Task<bool> IsEmailExistsAsync(string email, long? excludeId = null);
        Task<GuestViewModel?> FindByEmailAsync(string email);
        Task<GuestViewModel?> FindByPhoneAsync(string phone);
        Task<GuestViewModel?> FindByIdNumberAsync(string idNumber);
    }
}
