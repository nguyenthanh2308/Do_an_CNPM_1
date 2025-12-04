using System.Collections.Generic;
using System.Threading.Tasks;
using HotelManagementSystem.Models.ViewModels.RatePlan;

namespace HotelManagementSystem.Services.Interfaces
{
    public interface IRatePlanService
    {
        Task<List<RatePlanViewModel>> GetAllAsync(long? roomTypeId = null, long? hotelId = null);
        Task<RatePlanViewModel?> GetByIdAsync(long id);
        Task<RatePlanViewModel> CreateAsync(RatePlanViewModel model);
        Task<RatePlanViewModel?> UpdateAsync(RatePlanViewModel model);
        Task<bool> DeleteAsync(long id);
        Task<bool> RatePlanNameExistsAsync(long roomTypeId, string name, long? excludeId = null);
        Task<decimal> CalculatePriceAsync(long ratePlanId, DateTime checkIn, DateTime checkOut);
        Task<List<RatePlanViewModel>> GetActiveRatePlansAsync(long roomTypeId);
    }
}
