using HotelManagement.API.Data;
using HotelManagement.API.Models.Entities;
using HotelManagement.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.API.Repositories.Implementations
{
    public class BookingRepository : GenericRepository<Booking>, IBookingRepository
    {
        public BookingRepository(HotelDbContext context) : base(context)
        {
        }

        public async Task<Booking?> GetBookingWithDetailsAsync(long bookingId)
        {
            return await _dbSet
                .Include(b => b.Guest)
                .Include(b => b.BookingRooms)
                    .ThenInclude(br => br.Room)
                        .ThenInclude(r => r.RoomType)
                .Include(b => b.Payments)
                .Include(b => b.Invoice)
                .FirstOrDefaultAsync(b => b.Id == bookingId);
        }

        public async Task<IEnumerable<Booking>> GetBookingsByHotelAsync(long hotelId)
        {
            return await _dbSet
                .Where(b => b.HotelId == hotelId)
                .Include(b => b.Guest)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Booking>> GetBookingsByGuestAsync(long guestId)
        {
            return await _dbSet
                .Where(b => b.GuestId == guestId)
                .Include(b => b.Hotel)
                .OrderByDescending(b => b.CheckInDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Booking>> GetActiveBookingsAsync()
        {
            return await _dbSet
                .Where(b => b.Status == "Confirmed" || b.Status == "CheckedIn")
                .Include(b => b.Guest)
                .Include(b => b.Hotel)
                .ToListAsync();
        }

        public async Task<IEnumerable<Booking>> GetBookingsByDateRangeAsync(DateTime from, DateTime to)
        {
            return await _dbSet
                .Where(b => b.CheckInDate >= from && b.CheckInDate <= to)
                .Include(b => b.Guest)
                .Include(b => b.Hotel)
                .OrderBy(b => b.CheckInDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Booking>> GetPendingCheckInsAsync(DateTime date)
        {
            return await _dbSet
                .Where(b => b.Status == "Confirmed" && b.CheckInDate.Date == date.Date)
                .Include(b => b.Guest)
                .ToListAsync();
        }

        public async Task<IEnumerable<Booking>> GetPendingCheckOutsAsync(DateTime date)
        {
            return await _dbSet
                .Where(b => b.Status == "CheckedIn" && b.CheckOutDate.Date == date.Date)
                .Include(b => b.Guest)
                .ToListAsync();
        }
    }
}
