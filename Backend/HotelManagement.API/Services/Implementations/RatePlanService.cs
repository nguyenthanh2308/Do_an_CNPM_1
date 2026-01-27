using HotelManagement.API.Exceptions;
using HotelManagement.API.Models.DTOs.Common;
using HotelManagement.API.Models.Entities;
using HotelManagement.API.Repositories.Interfaces;
using HotelManagement.API.Services.Interfaces;

namespace HotelManagement.API.Services.Implementations
{
    public class RatePlanService : IRatePlanService
    {
        private readonly IUnitOfWork _unitOfWork;

        public RatePlanService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<RatePlan>> GetAllRatePlansAsync()
        {
            return await _unitOfWork.RatePlans.GetAllAsync();
        }

        public async Task<RatePlan?> GetRatePlanByIdAsync(long id)
        {
            return await _unitOfWork.RatePlans.GetByIdAsync(id);
        }

        public async Task<IEnumerable<RatePlan>> GetRatePlansByRoomTypeAsync(long roomTypeId)
        {
            return await _unitOfWork.RatePlans.GetRatePlansByRoomTypeAsync(roomTypeId);
        }

        public async Task<IEnumerable<RatePlan>> GetActiveRatePlansAsync(DateTime date)
        {
            return await _unitOfWork.RatePlans.GetActiveRatePlansAsync(date);
        }

        public async Task<RatePlan?> GetBestRatePlanAsync(long roomTypeId, DateTime checkIn, DateTime checkOut)
        {
            var ratePlans = await _unitOfWork.RatePlans.GetRatePlansByRoomTypeAsync(roomTypeId);
            
            // Filter active rate plans for the date range
            var activeRatePlans = ratePlans.Where(rp => 
                rp.StartDate <= checkIn && rp.EndDate >= checkOut
            ).ToList();

            if (!activeRatePlans.Any())
                return null;

            // Return the rate plan with lowest price
            return activeRatePlans.OrderBy(rp => rp.Price).FirstOrDefault();
        }

        public async Task<RatePlan> CreateRatePlanAsync(RatePlan ratePlan)
        {
            // Validate room type exists
            var roomType = await _unitOfWork.RoomTypes.GetByIdAsync(ratePlan.RoomTypeId);
            if (roomType == null)
                throw new NotFoundException("RoomType", ratePlan.RoomTypeId);

            // Validate dates
            if (ratePlan.StartDate >= ratePlan.EndDate)
                throw new ValidationException("Start date must be before end date.");

            // Validate price
            if (ratePlan.Price <= 0)
                throw new ValidationException("Price must be greater than zero.");

            ratePlan.CreatedAt = DateTime.Now;

            await _unitOfWork.RatePlans.AddAsync(ratePlan);
            await _unitOfWork.SaveChangesAsync();

            return ratePlan;
        }

        public async Task<RatePlan> UpdateRatePlanAsync(long id, RatePlan ratePlan)
        {
            var existingRatePlan = await _unitOfWork.RatePlans.GetByIdAsync(id);
            if (existingRatePlan == null)
                throw new NotFoundException("RatePlan", id);

            // Validate dates
            if (ratePlan.StartDate >= ratePlan.EndDate)
                throw new ValidationException("Start date must be before end date.");

            // Validate price
            if (ratePlan.Price <= 0)
                throw new ValidationException("Price must be greater than zero.");

            existingRatePlan.Name = ratePlan.Name;
            existingRatePlan.Type = ratePlan.Type;
            existingRatePlan.Price = ratePlan.Price;
            existingRatePlan.StartDate = ratePlan.StartDate;
            existingRatePlan.EndDate = ratePlan.EndDate;
            existingRatePlan.FreeCancelUntilHours = ratePlan.FreeCancelUntilHours;
            existingRatePlan.WeekendRuleJson = ratePlan.WeekendRuleJson;

            await _unitOfWork.RatePlans.UpdateAsync(existingRatePlan);
            await _unitOfWork.SaveChangesAsync();

            return existingRatePlan;
        }

        public async Task<bool> DeleteRatePlanAsync(long id)
        {
            var ratePlan = await _unitOfWork.RatePlans.GetByIdAsync(id);
            if (ratePlan == null)
                throw new NotFoundException("RatePlan", id);

            _unitOfWork.RatePlans.Delete(ratePlan);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<decimal> CalculateRateAsync(long ratePlanId, DateTime checkIn, DateTime checkOut)
        {
            var ratePlan = await _unitOfWork.RatePlans.GetByIdAsync(ratePlanId);
            if (ratePlan == null)
                throw new NotFoundException("RatePlan", ratePlanId);

            // Validate dates are within rate plan period
            if (checkIn < ratePlan.StartDate)
                throw new BusinessException("Check-in date is before rate plan start date.");

            if (checkOut > ratePlan.EndDate)
                throw new BusinessException("Check-out date is after rate plan end date.");

            // Calculate number of nights
            var nights = (checkOut - checkIn).Days;

            return ratePlan.Price * nights;
        }

        public async Task<PagedResult<RatePlan>> GetPagedRatePlansAsync(int pageNumber, int pageSize, long? roomTypeId = null)
        {
            var (items, totalCount) = await _unitOfWork.RatePlans.GetPagedAsync(
                pageNumber,
                pageSize,
                filter: roomTypeId.HasValue ? rp => rp.RoomTypeId == roomTypeId.Value : null,
                orderBy: query => query.OrderByDescending(rp => rp.CreatedAt)
            );

            return new PagedResult<RatePlan>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
    }
}
