using HotelManagement.API.Data;
using HotelManagement.API.Models.Entities;
using HotelManagement.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.API.Repositories.Implementations
{
    public class RoomRepository : GenericRepository<Room>, IRoomRepository
    {
        public RoomRepository(HotelDbContext context) : base(context)
        {
        }

        public async Task<Room?> GetRoomWithDetailsAsync(long roomId)
        {
            return await _dbSet
                .Include(r => r.Hotel)
                .Include(r => r.RoomType)
                    .ThenInclude(rt => rt.RoomTypeAmenities)
                        .ThenInclude(rta => rta.Amenity)
                .FirstOrDefaultAsync(r => r.Id == roomId);
        }

        public async Task<IEnumerable<Room>> GetRoomsByHotelAsync(long hotelId)
        {
            return await _dbSet
                .Where(r => r.HotelId == hotelId)
                .Include(r => r.RoomType)
                .OrderBy(r => r.Number)
                .ToListAsync();
        }

        public async Task<IEnumerable<Room>> GetRoomsByTypeAsync(long roomTypeId)
        {
            return await _dbSet
                .Where(r => r.RoomTypeId == roomTypeId)
                .Include(r => r.Hotel)
                .ToListAsync();
        }

        public async Task<IEnumerable<Room>> GetAvailableRoomsAsync(
            long hotelId,
            DateTime checkIn,
            DateTime checkOut,
            long? roomTypeId = null)
        {
            var query = _dbSet
                .Where(r => r.HotelId == hotelId && r.Status == "Vacant");

            if (roomTypeId.HasValue)
            {
                query = query.Where(r => r.RoomTypeId == roomTypeId.Value);
            }

            // Exclude rooms that have conflicting bookings
            var bookedRoomIds = await _context.BookingRooms
                .Include(br => br.Booking)
                .Where(br =>
                    br.Booking.HotelId == hotelId &&
                    br.Booking.Status != "Cancelled" &&
                    br.Booking.Status != "CheckedOut" &&
                    !(br.Booking.CheckOutDate <= checkIn || br.Booking.CheckInDate >= checkOut))
                .Select(br => br.RoomId)
                .Distinct()
                .ToListAsync();

            query = query.Where(r => !bookedRoomIds.Contains(r.Id));

            return await query
                .Include(r => r.RoomType)
                    .ThenInclude(rt => rt.RoomTypeAmenities)
                        .ThenInclude(rta => rta.Amenity)
                .ToListAsync();
        }

        public async Task<IEnumerable<Room>> GetRoomsByStatusAsync(string status)
        {
            return await _dbSet
                .Where(r => r.Status == status)
                .Include(r => r.Hotel)
                .Include(r => r.RoomType)
                .ToListAsync();
        }

        public async Task<Room?> GetRoomByNumberAsync(long hotelId, string roomNumber)
        {
            return await _dbSet
                .FirstOrDefaultAsync(r => r.HotelId == hotelId && r.Number == roomNumber);
        }

        public async Task UpdateRoomStatusAsync(long roomId, string status)
        {
            var room = await GetByIdAsync(roomId);
            if (room != null)
            {
                room.Status = status;
                Update(room);
            }
        }
    }
}
