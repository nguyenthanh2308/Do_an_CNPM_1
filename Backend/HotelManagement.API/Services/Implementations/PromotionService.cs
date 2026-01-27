using HotelManagement.API.Exceptions;
using HotelManagement.API.Models.DTOs.Common;
using HotelManagement.API.Models.Entities;
using HotelManagement.API.Repositories.Interfaces;
using HotelManagement.API.Services.Interfaces;

namespace HotelManagement.API.Services.Implementations
{
    public class PromotionService : IPromotionService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PromotionService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Promotion>> GetAllPromotionsAsync()
        {
            return await _unitOfWork.Promotions.GetAllAsync();
        }

        public async Task<Promotion?> GetPromotionByIdAsync(long id)
        {
            return await _unitOfWork.Promotions.GetByIdAsync(id);
        }

        public async Task<Promotion?> GetPromotionByCodeAsync(string code)
        {
            return await _unitOfWork.Promotions.GetPromotionByCodeAsync(code);
        }

        public async Task<IEnumerable<Promotion>> GetActivePromotionsAsync()
        {
            return await _unitOfWork.Promotions.GetActivePromotionsAsync();
        }

        public async Task<Promotion> CreatePromotionAsync(Promotion promotion)
        {
            // Validate dates
            if (promotion.StartDate >= promotion.EndDate)
                throw new ValidationException("Start date must be before end date.");

            // Check if code already exists
            var existing = await _unitOfWork.Promotions.GetPromotionByCodeAsync(promotion.Code);
            if (existing != null)
                throw new ValidationException($"Promotion code '{promotion.Code}' already exists.");

            // Validate value
            if (promotion.Type == "Percent" && (promotion.Value < 0 || promotion.Value > 100))
                throw new ValidationException("Percentage must be between 0 and 100.");

            if (promotion.Type == "Amount" && promotion.Value < 0)
                throw new ValidationException("Amount must be positive.");

            promotion.CreatedAt = DateTime.Now;

            await _unitOfWork.Promotions.AddAsync(promotion);
            await _unitOfWork.SaveChangesAsync();

            return promotion;
        }

        public async Task<Promotion> UpdatePromotionAsync(long id, Promotion promotion)
        {
            var existingPromotion = await _unitOfWork.Promotions.GetByIdAsync(id);
            if (existingPromotion == null)
                throw new NotFoundException("Promotion", id);

            // Validate dates
            if (promotion.StartDate >= promotion.EndDate)
                throw new ValidationException("Start date must be before end date.");

            // Check if code changed and new code already exists
            if (existingPromotion.Code != promotion.Code)
            {
                var codeExists = await _unitOfWork.Promotions.GetPromotionByCodeAsync(promotion.Code);
                if (codeExists != null)
                    throw new ValidationException($"Promotion code '{promotion.Code}' already exists.");
            }

            existingPromotion.Code = promotion.Code;
            existingPromotion.Type = promotion.Type;
            existingPromotion.Value = promotion.Value;
            existingPromotion.StartDate = promotion.StartDate;
            existingPromotion.EndDate = promotion.EndDate;
            existingPromotion.ConditionsJson = promotion.ConditionsJson;

            await _unitOfWork.Promotions.UpdateAsync(existingPromotion);
            await _unitOfWork.SaveChangesAsync();

            return existingPromotion;
        }

        public async Task<bool> DeletePromotionAsync(long id)
        {
            var promotion = await _unitOfWork.Promotions.GetByIdAsync(id);
            if (promotion == null)
                throw new NotFoundException("Promotion", id);

            _unitOfWork.Promotions.Delete(promotion);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ValidatePromotionAsync(string code, decimal bookingAmount)
        {
            var promotion = await _unitOfWork.Promotions.GetPromotionByCodeAsync(code);
            
            if (promotion == null)
                return false;

            var now = DateTime.Now;
            if (now < promotion.StartDate || now > promotion.EndDate)
                return false;

            return true;
        }

        public async Task<decimal> CalculateDiscountAsync(string code, decimal amount)
        {
            var promotion = await _unitOfWork.Promotions.GetPromotionByCodeAsync(code);
            
            if (promotion == null)
                throw new NotFoundException("Promotion", code);

            if (!await ValidatePromotionAsync(code, amount))
                throw new BusinessException("Promotion is not valid for this booking.");

            decimal discount = 0;

            if (promotion.Type == "Percent")
            {
                discount = amount * (promotion.Value / 100);
            }
            else if (promotion.Type == "Amount")
            {
                discount = promotion.Value;
            }

            return discount;
        }

        public async Task<PagedResult<Promotion>> GetPagedPromotionsAsync(int pageNumber, int pageSize, bool? activeOnly = null)
        {
            var now = DateTime.Now;
            var (items, totalCount) = await _unitOfWork.Promotions.GetPagedAsync(
                pageNumber,
                pageSize,
                filter: activeOnly == true ? p => p.StartDate <= now && p.EndDate >= now : null,
                orderBy: query => query.OrderByDescending(p => p.CreatedAt)
            );

            return new PagedResult<Promotion>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
    }
}
