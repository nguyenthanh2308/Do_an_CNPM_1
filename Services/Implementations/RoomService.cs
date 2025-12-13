using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotelManagementSystem.Data;
using HotelManagementSystem.Models.Entities;
using HotelManagementSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelManagementSystem.Services.Implementations
{
    public class RoomService : IRoomService
    {
        private readonly HotelDbContext _dbContext;

        public RoomService(HotelDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<Room>> GetAllAsync(long? hotelId = null, long? roomTypeId = null)
        {
            var query = _dbContext.Rooms
                .Include(r => r.Hotel)
                .Include(r => r.RoomType)
                .AsQueryable();

            if (hotelId.HasValue)
            {
                query = query.Where(r => r.HotelId == hotelId.Value);
            }

            if (roomTypeId.HasValue)
            {
                query = query.Where(r => r.RoomTypeId == roomTypeId.Value);
            }

            return await query
                .OrderBy(r => r.HotelId)
                .ThenBy(r => r.Number)
                .ToListAsync();
        }

        public async Task<Room?> GetByIdAsync(long id)
        {
            return await _dbContext.Rooms
                .Include(r => r.Hotel)
                .Include(r => r.RoomType)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<Room> CreateAsync(Room room)
        {
            // Có thể thêm validate: room number unique trong hotel
            _dbContext.Rooms.Add(room);
            await _dbContext.SaveChangesAsync();
            return room;
        }

        public async Task<bool> UpdateStatusAsync(long roomId, string newStatus)
        {
            var room = await _dbContext.Rooms.FindAsync(roomId);
            if (room == null)
            {
                return false;
            }

            // Bạn có thể thêm validate: chỉ cho phép 4 trạng thái trong DB:
            // Vacant, Occupied, Cleaning, Maintenance
            room.Status = newStatus;
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(long id)
        {
            var room = await _dbContext.Rooms.FindAsync(id);
            if (room == null)
            {
                return false;
            }

            _dbContext.Rooms.Remove(room);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<Room?> UpdateAsync(Room room)
        {
            var existing = await _dbContext.Rooms.FindAsync(room.Id);
            if (existing == null)
            {
                return null;
            }

            // Cập nhật các field mong muốn
            existing.HotelId = room.HotelId;
            existing.RoomTypeId = room.RoomTypeId;
            existing.Number = room.Number;
            existing.Floor = room.Floor;
            existing.Status = room.Status;
            existing.ImageUrl = room.ImageUrl;

            await _dbContext.SaveChangesAsync();
            return existing;
        }

        /// <summary>
        /// Tìm phòng trống theo khoảng ngày + số người.
        /// Logic:
        /// - Chọn Room có RoomType.Capacity >= numberOfGuests
        /// - Không có bất kỳ Booking nào (Confirmed/CheckedIn)
        ///   giao nhau với khoảng [checkInDate, checkOutDate).
        /// </summary>
        public async Task<List<Room>> SearchAvailableRoomsAsync(
            DateTime checkInDate,
            DateTime checkOutDate,
            int numberOfGuests,
            long? hotelId = null,
            long? roomTypeId = null)
        {
            if (checkInDate >= checkOutDate)
            {
                throw new ArgumentException("Check-in date must be earlier than check-out date.");
            }

            // Base query: lọc theo sức chứa + hotel/roomType nếu có
            var query = _dbContext.Rooms
                .Include(r => r.RoomType)
                .Include(r => r.Hotel)
                .Where(r => r.RoomType.Capacity >= numberOfGuests)
                .AsQueryable();

            if (hotelId.HasValue)
            {
                query = query.Where(r => r.HotelId == hotelId.Value);
            }

            if (roomTypeId.HasValue)
            {
                query = query.Where(r => r.RoomTypeId == roomTypeId.Value);
            }

            // Loại các phòng đã bị chiếm:
            // tồn tại BookingRoom có Booking (Confirmed/CheckedIn)
            // với khoảng ngày giao nhau với [checkInDate, checkOutDate)
            query = query.Where(room =>
                !_dbContext.BookingRooms.Any(br =>
                    br.RoomId == room.Id &&
                    br.Booking != null &&
                    (br.Booking.Status == "Confirmed" || br.Booking.Status == "CheckedIn") &&
                    br.Booking.CheckInDate < checkOutDate &&
                    br.Booking.CheckOutDate > checkInDate
                )
            );

            return await query
                .OrderBy(r => r.HotelId)
                .ThenBy(r => r.Number)
                .ToListAsync();
        }
    }
}
