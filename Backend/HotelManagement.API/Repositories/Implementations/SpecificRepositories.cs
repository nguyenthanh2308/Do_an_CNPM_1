using HotelManagement.API.Data;
using HotelManagement.API.Models.Entities;
using HotelManagement.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.API.Repositories.Implementations
{
    public class GuestRepository : GenericRepository<Guest>, IGuestRepository
    {
        public GuestRepository(HotelDbContext context) : base(context) { }

        public async Task<Guest?> GetGuestByIdentityNumberAsync(string identityNumber)
        {
            return await _dbSet.FirstOrDefaultAsync(g => g.IdNumber == identityNumber);
        }

        public async Task<IEnumerable<Guest>> SearchGuestsByNameAsync(string name)
        {
            return await _dbSet
                .Where(g => g.FullName.Contains(name))
                .ToListAsync();
        }
    }

    public class HotelRepository : GenericRepository<Hotel>, IHotelRepository
    {
        public HotelRepository(HotelDbContext context) : base(context) { }

        public async Task<Hotel?> GetHotelWithDetailsAsync(long hotelId)
        {
            return await _dbSet
                .Include(h => h.RoomTypes)
                .Include(h => h.Rooms)
                .FirstOrDefaultAsync(h => h.Id == hotelId);
        }
    }

    public class RoomTypeRepository : GenericRepository<RoomType>, IRoomTypeRepository
    {
        public RoomTypeRepository(HotelDbContext context) : base(context) { }

        public async Task<IEnumerable<RoomType>> GetRoomTypesByHotelAsync(long hotelId)
        {
            return await _dbSet
                .Where(rt => rt.HotelId == hotelId)
                .Include(rt => rt.RoomTypeAmenities)
                    .ThenInclude(rta => rta.Amenity)
                .ToListAsync();
        }

        public async Task<RoomType?> GetRoomTypeWithAmenitiesAsync(long roomTypeId)
        {
            return await _dbSet
                .Include(rt => rt.RoomTypeAmenities)
                    .ThenInclude(rta => rta.Amenity)
                .FirstOrDefaultAsync(rt => rt.Id == roomTypeId);
        }
    }

    public class PaymentRepository : GenericRepository<Payment>, IPaymentRepository
    {
        public PaymentRepository(HotelDbContext context) : base(context) { }

        public async Task<IEnumerable<Payment>> GetPaymentsByBookingAsync(long bookingId)
        {
            return await _dbSet
                .Where(p => p.BookingId == bookingId)
                .OrderBy(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalPaidAsync(long bookingId)
        {
            return await _dbSet
                .Where(p => p.BookingId == bookingId && p.Status == "Paid")
                .SumAsync(p => p.Amount);
        }
    }

    public class InvoiceRepository : GenericRepository<Invoice>, IInvoiceRepository
    {
        public InvoiceRepository(HotelDbContext context) : base(context) { }

        public async Task<Invoice?> GetInvoiceByBookingAsync(long bookingId)
        {
            return await _dbSet
                .Include(i => i.Booking)
                    .ThenInclude(b => b.Guest)
                .FirstOrDefaultAsync(i => i.BookingId == bookingId);
        }
    }

    public class HousekeepingRepository : GenericRepository<HousekeepingTask>, IHousekeepingRepository
    {
        public HousekeepingRepository(HotelDbContext context) : base(context) { }

        public async Task<IEnumerable<HousekeepingTask>> GetTasksByUserAsync(long userId)
        {
            return await _dbSet
                .Where(t => t.AssignedToUserId == userId)
                .Include(t => t.Room)
                .OrderBy(t => t.ScheduledAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<HousekeepingTask>> GetTasksByRoomAsync(long roomId)
        {
            return await _dbSet
                .Where(t => t.RoomId == roomId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<HousekeepingTask>> GetTasksByStatusAsync(string status)
        {
            return await _dbSet
                .Where(t => t.Status == status)
                .Include(t => t.Room)
                .Include(t => t.AssignedToUser)
                .ToListAsync();
        }

        public async Task<IEnumerable<HousekeepingTask>> GetPendingTasksAsync()
        {
            return await _dbSet
                .Where(t => t.Status == "Pending" || t.Status == "InProgress")
                .Include(t => t.Room)
                .Include(t => t.AssignedToUser)
                .OrderBy(t => t.Priority)
                .ThenBy(t => t.ScheduledAt)
                .ToListAsync();
        }
    }

    public class PromotionRepository : GenericRepository<Promotion>, IPromotionRepository
    {
        public PromotionRepository(HotelDbContext context) : base(context) { }

        public async Task<Promotion?> GetPromotionByCodeAsync(string code)
        {
            return await _dbSet.FirstOrDefaultAsync(p => p.Code == code);
        }

        public async Task<IEnumerable<Promotion>> GetActivePromotionsAsync()
        {
            var now = DateTime.Now;
            return await _dbSet
                .Where(p => p.StartDate <= now && p.EndDate >= now)
                .ToListAsync();
        }
    }

    public class RatePlanRepository : GenericRepository<RatePlan>, IRatePlanRepository
    {
        public RatePlanRepository(HotelDbContext context) : base(context) { }

        public async Task<IEnumerable<RatePlan>> GetRatePlansByRoomTypeAsync(long roomTypeId)
        {
            return await _dbSet
                .Where(rp => rp.RoomTypeId == roomTypeId)
                .ToListAsync();
        }

        public async Task<IEnumerable<RatePlan>> GetActiveRatePlansAsync(DateTime date)
        {
            return await _dbSet
                .Where(rp => rp.StartDate <= date && rp.EndDate >= date)
                .Include(rp => rp.RoomType)
                .ToListAsync();
        }
    }

    public class AmenityRepository : GenericRepository<Amenity>, IAmenityRepository
    {
        public AmenityRepository(HotelDbContext context) : base(context) { }

        public async Task<IEnumerable<Amenity>> GetAmenitiesByRoomTypeAsync(long roomTypeId)
        {
            return await _context.RoomTypeAmenities
                .Where(rta => rta.RoomTypeId == roomTypeId)
                .Select(rta => rta.Amenity)
                .ToListAsync();
        }
    }
}
