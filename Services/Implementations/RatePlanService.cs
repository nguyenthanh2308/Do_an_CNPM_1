using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotelManagementSystem.Data;
using HotelManagementSystem.Models.Entities;
using HotelManagementSystem.Models.ViewModels.RatePlan;
using HotelManagementSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelManagementSystem.Services.Implementations
{
    public class RatePlanService : IRatePlanService
    {
        private readonly HotelDbContext _context;

        public RatePlanService(HotelDbContext context)
        {
            _context = context;
        }

        public async Task<List<RatePlanViewModel>> GetAllAsync(long? roomTypeId = null, long? hotelId = null)
        {
            var query = _context.RatePlans
                .Include(rp => rp.RoomType)
                    .ThenInclude(rt => rt.Hotel)
                .AsQueryable();

            if (roomTypeId.HasValue)
            {
                query = query.Where(rp => rp.RoomTypeId == roomTypeId.Value);
            }

            if (hotelId.HasValue)
            {
                query = query.Where(rp => rp.RoomType.HotelId == hotelId.Value);
            }

            var ratePlans = await query
                .OrderBy(rp => rp.RoomType.Hotel.Name)
                .ThenBy(rp => rp.RoomType.Name)
                .ThenBy(rp => rp.StartDate)
                .ToListAsync();

            return ratePlans.Select(rp => new RatePlanViewModel
            {
                Id = rp.Id,
                RoomTypeId = rp.RoomTypeId,
                RoomTypeName = rp.RoomType.Name,
                HotelName = rp.RoomType.Hotel.Name,
                BasePrice = rp.RoomType.BasePrice,
                Name = rp.Name,
                Type = rp.Type,
                FreeCancelUntilHours = rp.FreeCancelUntilHours,
                StartDate = rp.StartDate,
                EndDate = rp.EndDate,
                Price = rp.Price,
                IsWeekendRateActive = !string.IsNullOrEmpty(rp.WeekendRuleJson),
                WeekendAdjustmentPercent = ParseWeekendAdjustment(rp.WeekendRuleJson),
                CreatedAt = rp.CreatedAt
            }).ToList();
        }

        public async Task<RatePlanViewModel?> GetByIdAsync(long id)
        {
            var ratePlan = await _context.RatePlans
                .Include(rp => rp.RoomType)
                    .ThenInclude(rt => rt.Hotel)
                .FirstOrDefaultAsync(rp => rp.Id == id);

            if (ratePlan == null)
            {
                return null;
            }

            return new RatePlanViewModel
            {
                Id = ratePlan.Id,
                RoomTypeId = ratePlan.RoomTypeId,
                RoomTypeName = ratePlan.RoomType.Name,
                HotelName = ratePlan.RoomType.Hotel.Name,
                BasePrice = ratePlan.RoomType.BasePrice,
                Name = ratePlan.Name,
                Type = ratePlan.Type,
                FreeCancelUntilHours = ratePlan.FreeCancelUntilHours,
                StartDate = ratePlan.StartDate,
                EndDate = ratePlan.EndDate,
                Price = ratePlan.Price,
                IsWeekendRateActive = !string.IsNullOrEmpty(ratePlan.WeekendRuleJson),
                WeekendAdjustmentPercent = ParseWeekendAdjustment(ratePlan.WeekendRuleJson),
                CreatedAt = ratePlan.CreatedAt
            };
        }

        public async Task<RatePlanViewModel> CreateAsync(RatePlanViewModel model)
        {
            // Validate room type exists
            var roomType = await _context.RoomTypes.FindAsync(model.RoomTypeId);
            if (roomType == null)
            {
                throw new InvalidOperationException("Loại phòng không tồn tại.");
            }

            // Check duplicate name for same room type and overlapping dates
            if (await RatePlanNameExistsAsync(model.RoomTypeId, model.Name))
            {
                throw new InvalidOperationException($"Rate plan '{model.Name}' đã tồn tại cho loại phòng này.");
            }

            // Validate type
            ValidateType(model);

            // Validate date range
            if (model.StartDate > model.EndDate)
            {
                throw new InvalidOperationException("Ngày bắt đầu phải nhỏ hơn hoặc bằng ngày kết thúc.");
            }

            var ratePlan = new Models.Entities.RatePlan
            {
                RoomTypeId = model.RoomTypeId,
                Name = model.Name,
                Type = model.Type,
                FreeCancelUntilHours = model.FreeCancelUntilHours,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                Price = model.Price,
                WeekendRuleJson = BuildWeekendRuleJson(model),
                CreatedAt = DateTime.Now
            };

            _context.RatePlans.Add(ratePlan);
            await _context.SaveChangesAsync();

            model.Id = ratePlan.Id;
            model.CreatedAt = ratePlan.CreatedAt;
            return model;
        }

        public async Task<RatePlanViewModel?> UpdateAsync(RatePlanViewModel model)
        {
            var ratePlan = await _context.RatePlans.FindAsync(model.Id);
            if (ratePlan == null)
            {
                return null;
            }

            // Check duplicate name
            if (await RatePlanNameExistsAsync(model.RoomTypeId, model.Name, model.Id))
            {
                throw new InvalidOperationException($"Rate plan '{model.Name}' đã tồn tại cho loại phòng này.");
            }

            // Validate type
            ValidateType(model);

            // Validate date range
            if (model.StartDate > model.EndDate)
            {
                throw new InvalidOperationException("Ngày bắt đầu phải nhỏ hơn hoặc bằng ngày kết thúc.");
            }

            ratePlan.RoomTypeId = model.RoomTypeId;
            ratePlan.Name = model.Name;
            ratePlan.Type = model.Type;
            ratePlan.FreeCancelUntilHours = model.FreeCancelUntilHours;
            ratePlan.StartDate = model.StartDate;
            ratePlan.EndDate = model.EndDate;
            ratePlan.Price = model.Price;
            ratePlan.WeekendRuleJson = BuildWeekendRuleJson(model);

            await _context.SaveChangesAsync();
            return model;
        }

        public async Task<bool> DeleteAsync(long id)
        {
            var ratePlan = await _context.RatePlans.FindAsync(id);

            if (ratePlan == null)
            {
                return false;
            }

            // Note: Không thể kiểm tra bookings vì không có FK relationship
            // Thông tin rate plan được lưu dưới dạng snapshot trong Booking.RatePlanSnapshotJson
            // Cho phép xóa rate plan ngay cả khi đã được sử dụng trong bookings trước đó

            _context.RatePlans.Remove(ratePlan);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RatePlanNameExistsAsync(long roomTypeId, string name, long? excludeId = null)
        {
            var query = _context.RatePlans
                .Where(rp => rp.RoomTypeId == roomTypeId && rp.Name == name);

            if (excludeId.HasValue)
            {
                query = query.Where(rp => rp.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<decimal> CalculatePriceAsync(long ratePlanId, DateTime checkIn, DateTime checkOut)
        {
            var ratePlan = await _context.RatePlans
                .FirstOrDefaultAsync(rp => rp.Id == ratePlanId);

            if (ratePlan == null)
            {
                throw new InvalidOperationException("Rate plan không tồn tại.");
            }

            var totalPrice = 0m;
            var currentDate = checkIn.Date;

            while (currentDate < checkOut.Date)
            {
                var isWeekend = IsWeekend(currentDate);
                var dailyPrice = ratePlan.Price;

                // Apply weekend adjustment if applicable
                if (isWeekend && !string.IsNullOrEmpty(ratePlan.WeekendRuleJson))
                {
                    var weekendAdjustment = ParseWeekendAdjustment(ratePlan.WeekendRuleJson);
                    if (weekendAdjustment.HasValue)
                    {
                        dailyPrice = dailyPrice * (1 + weekendAdjustment.Value / 100);
                    }
                }

                totalPrice += dailyPrice;
                currentDate = currentDate.AddDays(1);
            }

            return Math.Round(totalPrice, 2);
        }

        public async Task<List<RatePlanViewModel>> GetActiveRatePlansAsync(long roomTypeId)
        {
            var now = DateTime.Now.Date;
            var ratePlans = await _context.RatePlans
                .Include(rp => rp.RoomType)
                    .ThenInclude(rt => rt.Hotel)
                .Where(rp => rp.RoomTypeId == roomTypeId 
                    && rp.StartDate <= now
                    && rp.EndDate >= now)
                .OrderBy(rp => rp.Price)
                .ToListAsync();

            return ratePlans.Select(rp => new RatePlanViewModel
            {
                Id = rp.Id,
                RoomTypeId = rp.RoomTypeId,
                RoomTypeName = rp.RoomType.Name,
                HotelName = rp.RoomType.Hotel.Name,
                BasePrice = rp.RoomType.BasePrice,
                Name = rp.Name,
                Type = rp.Type,
                FreeCancelUntilHours = rp.FreeCancelUntilHours,
                StartDate = rp.StartDate,
                EndDate = rp.EndDate,
                Price = rp.Price,
                IsWeekendRateActive = !string.IsNullOrEmpty(rp.WeekendRuleJson),
                WeekendAdjustmentPercent = ParseWeekendAdjustment(rp.WeekendRuleJson),
                CreatedAt = rp.CreatedAt
            }).ToList();
        }

        // Helper methods
        private void ValidateType(RatePlanViewModel model)
        {
            var validTypes = new[] { "Flexible", "NonRefundable" };
            if (!validTypes.Contains(model.Type))
            {
                throw new InvalidOperationException("Loại rate plan không hợp lệ. Chỉ chấp nhận 'Flexible' hoặc 'NonRefundable'.");
            }

            // Validate FreeCancelUntilHours based on type
            if (model.Type == "Flexible")
            {
                if (!model.FreeCancelUntilHours.HasValue || model.FreeCancelUntilHours.Value < 1)
                {
                    throw new InvalidOperationException(
                        "Loại 'Flexible' yêu cầu thời gian hủy miễn phí ít nhất 1 giờ.");
                }
            }
            else if (model.Type == "NonRefundable")
            {
                model.FreeCancelUntilHours = null; // Force null for NonRefundable
            }
        }

        private string? BuildWeekendRuleJson(RatePlanViewModel model)
        {
            if (!model.IsWeekendRateActive || !model.WeekendAdjustmentPercent.HasValue)
            {
                return null;
            }

            // Simple JSON format: {"adjustment_percent": 20}
            return $"{{\"adjustment_percent\": {model.WeekendAdjustmentPercent.Value}}}";
        }

        private decimal? ParseWeekendAdjustment(string? weekendRuleJson)
        {
            if (string.IsNullOrEmpty(weekendRuleJson))
            {
                return null;
            }

            try
            {
                // Simple parsing: {"adjustment_percent": 20}
                var json = weekendRuleJson.Trim();
                var start = json.IndexOf(":") + 1;
                var end = json.IndexOf("}");
                if (start > 0 && end > start)
                {
                    var valueStr = json.Substring(start, end - start).Trim();
                    if (decimal.TryParse(valueStr, out var value))
                    {
                        return value;
                    }
                }
            }
            catch
            {
                // Ignore parsing errors
            }

            return null;
        }

        private bool IsWeekend(DateTime date)
        {
            return date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
        }
    }
}
