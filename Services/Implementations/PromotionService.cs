using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotelManagementSystem.Data;
using HotelManagementSystem.Models.Entities;
using HotelManagementSystem.Models.ViewModels.Promotion;
using HotelManagementSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelManagementSystem.Services.Implementations
{
    public class PromotionService : IPromotionService
    {
        private readonly HotelDbContext _context;

        public PromotionService(HotelDbContext context)
        {
            _context = context;
        }

        public async Task<List<PromotionViewModel>> GetAllAsync()
        {
            var promotions = await _context.Promotions
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return promotions.Select(MapToViewModel).ToList();
        }

        public async Task<PromotionViewModel?> GetByIdAsync(long id)
        {
            var promotion = await _context.Promotions.FindAsync(id);
            return promotion == null ? null : MapToViewModel(promotion);
        }

        public async Task<PromotionViewModel?> GetByCodeAsync(string code)
        {
            var promotion = await _context.Promotions
                .FirstOrDefaultAsync(p => p.Code == code.ToUpper());
            
            return promotion == null ? null : MapToViewModel(promotion);
        }

        public async Task<bool> CreateAsync(PromotionViewModel model)
        {
            try
            {
                // Validate
                if (!ValidateModel(model))
                    return false;

                // Check duplicate code
                if (await IsCodeExistsAsync(model.Code))
                    return false;

                var promotion = new Promotion
                {
                    Code = model.Code.ToUpper(),
                    Type = model.Type,
                    Value = model.Value,
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,
                    ConditionsJson = model.BuildConditionsJson(),
                    CreatedAt = DateTime.Now
                };

                _context.Promotions.Add(promotion);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateAsync(PromotionViewModel model)
        {
            try
            {
                var promotion = await _context.Promotions.FindAsync(model.Id);
                if (promotion == null)
                    return false;

                // Validate
                if (!ValidateModel(model))
                    return false;

                // Check duplicate code (exclude current)
                if (await IsCodeExistsAsync(model.Code, model.Id))
                    return false;

                promotion.Code = model.Code.ToUpper();
                promotion.Type = model.Type;
                promotion.Value = model.Value;
                promotion.StartDate = model.StartDate;
                promotion.EndDate = model.EndDate;
                promotion.ConditionsJson = model.BuildConditionsJson();

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteAsync(long id)
        {
            try
            {
                var promotion = await _context.Promotions.FindAsync(id);
                if (promotion == null)
                    return false;

                // Note: Database doesn't have promotion_code field in bookings table
                // Promotions can be safely deleted
                
                _context.Promotions.Remove(promotion);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> IsCodeExistsAsync(string code, long? excludeId = null)
        {
            var query = _context.Promotions.Where(p => p.Code == code.ToUpper());
            
            if (excludeId.HasValue)
            {
                query = query.Where(p => p.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<bool> ValidatePromotionAsync(string code, decimal orderAmount, bool isNewCustomer)
        {
            var promotion = await GetByCodeAsync(code);
            if (promotion == null)
                return false;

            return promotion.CanApplyToOrder(orderAmount, isNewCustomer);
        }

        public async Task<decimal> CalculateDiscountAsync(string code, decimal orderAmount)
        {
            var promotion = await GetByCodeAsync(code);
            if (promotion == null)
                return 0;

            if (!promotion.IsCurrentlyActive || promotion.IsUsageLimitReached)
                return 0;

            return promotion.CalculateDiscount(orderAmount);
        }

        public async Task<bool> IncrementUsageCountAsync(string code)
        {
            try
            {
                var promotion = await _context.Promotions
                    .FirstOrDefaultAsync(p => p.Code == code.ToUpper());

                if (promotion == null)
                    return false;

                // Parse current conditions
                var viewModel = MapToViewModel(promotion);
                viewModel.CurrentUsageCount++;

                // Update conditions JSON with new count
                promotion.ConditionsJson = viewModel.BuildConditionsJson();

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Private helper methods
        private bool ValidateModel(PromotionViewModel model)
        {
            // Validate dates
            if (model.EndDate < model.StartDate)
                return false;

            // Validate type
            if (model.Type != "Percent" && model.Type != "Amount")
                return false;

            // Validate value based on type
            if (model.Type == "Percent" && (model.Value <= 0 || model.Value > 100))
                return false;

            if (model.Type == "Amount" && model.Value <= 0)
                return false;

            // Validate max discount for percent type
            if (model.Type == "Percent" && model.MaxDiscountAmount.HasValue)
            {
                if (model.MaxDiscountAmount.Value <= 0)
                    return false;
            }

            return true;
        }

        private PromotionViewModel MapToViewModel(Promotion promotion)
        {
            var viewModel = new PromotionViewModel
            {
                Id = promotion.Id,
                Code = promotion.Code,
                Type = promotion.Type,
                Value = promotion.Value,
                StartDate = promotion.StartDate,
                EndDate = promotion.EndDate,
                CreatedAt = promotion.CreatedAt
            };

            // Parse conditions JSON
            viewModel.ParseConditionsJson(promotion.ConditionsJson);

            return viewModel;
        }
    }
}
