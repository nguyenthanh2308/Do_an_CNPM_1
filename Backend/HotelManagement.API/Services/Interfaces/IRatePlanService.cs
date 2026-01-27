using HotelManagement.API.Models.DTOs.Common;
using HotelManagement.API.Models.Entities;

namespace HotelManagement.API.Services.Interfaces
{
    public interface IRatePlanService
    {
        Task<IEnumerable<RatePlan>> GetAllRatePlansAsync();
        Task<RatePlan?> GetRatePlanByIdAsync(long id);
        Task<IEnumerable<RatePlan>> GetRatePlansByRoomTypeAsync(long roomTypeId);
        Task<IEnumerable<RatePlan>> GetActiveRatePlansAsync(DateTime date);
        Task<RatePlan?> GetBestRatePlanAsync(long roomTypeId, DateTime checkIn, DateTime checkOut);
        Task<RatePlan> CreateRatePlanAsync(RatePlan ratePlan);
        Task<RatePlan> UpdateRatePlanAsync(long id, RatePlan ratePlan);
        Task<bool> DeleteRatePlanAsync(long id);
        Task<decimal> CalculateRateAsync(long ratePlanId, DateTime checkIn, DateTime checkOut);
        Task<PagedResult<RatePlan>> GetPagedRatePlansAsync(int pageNumber, int pageSize, long? roomTypeId = null);
    }
}
