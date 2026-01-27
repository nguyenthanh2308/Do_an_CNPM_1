using HotelManagement.API.Models.DTOs.Common;
using HotelManagement.API.Models.Entities;

namespace HotelManagement.API.Services.Interfaces
{
    public interface IPromotionService
    {
        Task<IEnumerable<Promotion>> GetAllPromotionsAsync();
        Task<Promotion?> GetPromotionByIdAsync(long id);
        Task<Promotion?> GetPromotionByCodeAsync(string code);
        Task<IEnumerable<Promotion>> GetActivePromotionsAsync();
        Task<Promotion> CreatePromotionAsync(Promotion promotion);
        Task<Promotion> UpdatePromotionAsync(long id, Promotion promotion);
        Task<bool> DeletePromotionAsync(long id);
        Task<bool> ValidatePromotionAsync(string code, decimal bookingAmount);
        Task<decimal> CalculateDiscountAsync(string code, decimal amount);
        Task<PagedResult<Promotion>> GetPagedPromotionsAsync(int pageNumber, int pageSize, bool? activeOnly = null);
    }
}
