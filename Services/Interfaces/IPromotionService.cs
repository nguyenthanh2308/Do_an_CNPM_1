using System.Collections.Generic;
using System.Threading.Tasks;
using HotelManagementSystem.Models.ViewModels.Promotion;

namespace HotelManagementSystem.Services.Interfaces
{
    public interface IPromotionService
    {
        Task<List<PromotionViewModel>> GetAllAsync();
        Task<PromotionViewModel?> GetByIdAsync(long id);
        Task<PromotionViewModel?> GetByCodeAsync(string code);
        Task<bool> CreateAsync(PromotionViewModel model);
        Task<bool> UpdateAsync(PromotionViewModel model);
        Task<bool> DeleteAsync(long id);
        Task<bool> IsCodeExistsAsync(string code, long? excludeId = null);
        Task<bool> ValidatePromotionAsync(string code, decimal orderAmount, bool isNewCustomer);
        Task<decimal> CalculateDiscountAsync(string code, decimal orderAmount);
        Task<bool> IncrementUsageCountAsync(string code);
    }
}
