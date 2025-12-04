using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotelManagementSystem.Data;
using HotelManagementSystem.Models.Entities;
using HotelManagementSystem.Models.ViewModels.Hotel;
using HotelManagementSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelManagementSystem.Services.Implementations
{
    public class HotelService : IHotelService
    {
        private readonly HotelDbContext _context;

        public HotelService(HotelDbContext context)
        {
            _context = context;
        }

        public async Task<List<HotelViewModel>> GetAllAsync()
        {
            var hotels = await _context.Hotels
                .Include(h => h.RoomTypes)
                .Include(h => h.Rooms)
                .OrderByDescending(h => h.CreatedAt)
                .ToListAsync();

            return hotels.Select(h => new HotelViewModel
            {
                Id = h.Id,
                Name = h.Name,
                Address = h.Address,
                Timezone = h.Timezone,
                CreatedAt = h.CreatedAt,
                RoomTypeCount = h.RoomTypes.Count,
                TotalRooms = h.Rooms.Count,
                TotalBookings = h.Bookings.Count
            }).ToList();
        }

        public async Task<HotelViewModel?> GetByIdAsync(long id)
        {
            var hotel = await _context.Hotels
                .Include(h => h.RoomTypes)
                .Include(h => h.Rooms)
                .FirstOrDefaultAsync(h => h.Id == id);

            if (hotel == null)
            {
                return null;
            }

            return new HotelViewModel
            {
                Id = hotel.Id,
                Name = hotel.Name,
                Address = hotel.Address,
                Timezone = hotel.Timezone,
                CreatedAt = hotel.CreatedAt,
                RoomTypeCount = hotel.RoomTypes.Count,
                TotalRooms = hotel.Rooms.Count,
                TotalBookings = hotel.Bookings.Count
            };
        }

        public async Task<Hotel?> GetHotelEntityByIdAsync(long id)
        {
            return await _context.Hotels.FindAsync(id);
        }

        public async Task<Hotel> CreateAsync(HotelViewModel model)
        {
            // Kiểm tra tên khách sạn đã tồn tại
            if (await HotelNameExistsAsync(model.Name))
            {
                throw new InvalidOperationException($"Khách sạn '{model.Name}' đã tồn tại");
            }

            var hotel = new Hotel
            {
                Name = model.Name,
                Address = model.Address,
                Timezone = model.Timezone,
                CreatedAt = DateTime.Now
            };

            _context.Hotels.Add(hotel);
            await _context.SaveChangesAsync();

            return hotel;
        }

        public async Task<Hotel?> UpdateAsync(HotelViewModel model)
        {
            var hotel = await _context.Hotels.FindAsync(model.Id);
            if (hotel == null)
            {
                return null;
            }

            // Kiểm tra tên khách sạn đã tồn tại (trừ chính nó)
            if (await HotelNameExistsAsync(model.Name, model.Id))
            {
                throw new InvalidOperationException($"Khách sạn '{model.Name}' đã tồn tại");
            }

            hotel.Name = model.Name;
            hotel.Address = model.Address;
            hotel.Timezone = model.Timezone;

            await _context.SaveChangesAsync();

            return hotel;
        }

        public async Task<bool> DeleteAsync(long id)
        {
            var hotel = await _context.Hotels
                .Include(h => h.Rooms)
                .Include(h => h.RoomTypes)
                .FirstOrDefaultAsync(h => h.Id == id);

            if (hotel == null)
            {
                return false;
            }

            // Kiểm tra ràng buộc: Không được xóa nếu có rooms hoặc room types
            if (hotel.Rooms.Any())
            {
                throw new InvalidOperationException("Không thể xóa khách sạn đang có phòng. Vui lòng xóa tất cả phòng trước.");
            }

            if (hotel.RoomTypes.Any())
            {
                throw new InvalidOperationException("Không thể xóa khách sạn đang có loại phòng. Vui lòng xóa tất cả loại phòng trước.");
            }

            // Check bookings count separately without Include
            var bookingCount = await _context.Bookings.CountAsync(b => b.HotelId == id);
            if (bookingCount > 0)
            {
                throw new InvalidOperationException("Không thể xóa khách sạn đang có booking. Vui lòng xử lý tất cả booking trước.");
            }

            _context.Hotels.Remove(hotel);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> HotelNameExistsAsync(string name, long? excludeId = null)
        {
            var query = _context.Hotels.Where(h => h.Name == name);

            if (excludeId.HasValue)
            {
                query = query.Where(h => h.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }
    }
}
