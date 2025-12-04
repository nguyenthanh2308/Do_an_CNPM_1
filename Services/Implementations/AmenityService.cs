using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotelManagementSystem.Data;
using HotelManagementSystem.Models.Entities;
using HotelManagementSystem.Models.ViewModels.Amenity;
using HotelManagementSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelManagementSystem.Services.Implementations
{
    public class AmenityService : IAmenityService
    {
        private readonly HotelDbContext _context;

        public AmenityService(HotelDbContext context)
        {
            _context = context;
        }

        public async Task<List<AmenityViewModel>> GetAllAsync()
        {
            var amenities = await _context.Amenities
                .Include(a => a.RoomTypeAmenities)
                .OrderBy(a => a.Name)
                .ToListAsync();

            return amenities.Select(a => new AmenityViewModel
            {
                Id = a.Id,
                Name = a.Name,
                UsageCount = a.RoomTypeAmenities.Count
            }).ToList();
        }

        public async Task<AmenityViewModel?> GetByIdAsync(long id)
        {
            var amenity = await _context.Amenities
                .Include(a => a.RoomTypeAmenities)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (amenity == null)
            {
                return null;
            }

            return new AmenityViewModel
            {
                Id = amenity.Id,
                Name = amenity.Name,
                UsageCount = amenity.RoomTypeAmenities.Count
            };
        }

        public async Task<Amenity?> GetAmenityEntityByIdAsync(long id)
        {
            return await _context.Amenities.FindAsync(id);
        }

        public async Task<Amenity> CreateAsync(AmenityViewModel model)
        {
            if (await AmenityNameExistsAsync(model.Name))
            {
                throw new InvalidOperationException($"Tiện nghi '{model.Name}' đã tồn tại");
            }

            var amenity = new Amenity
            {
                Name = model.Name
            };

            _context.Amenities.Add(amenity);
            await _context.SaveChangesAsync();

            return amenity;
        }

        public async Task<Amenity?> UpdateAsync(AmenityViewModel model)
        {
            var amenity = await _context.Amenities.FindAsync(model.Id);
            if (amenity == null)
            {
                return null;
            }

            if (await AmenityNameExistsAsync(model.Name, model.Id))
            {
                throw new InvalidOperationException($"Tiện nghi '{model.Name}' đã tồn tại");
            }

            amenity.Name = model.Name;
            await _context.SaveChangesAsync();

            return amenity;
        }

        public async Task<bool> DeleteAsync(long id)
        {
            var amenity = await _context.Amenities
                .Include(a => a.RoomTypeAmenities)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (amenity == null)
            {
                return false;
            }

            if (amenity.RoomTypeAmenities.Any())
            {
                throw new InvalidOperationException("Không thể xóa tiện nghi đang được sử dụng bởi loại phòng");
            }

            _context.Amenities.Remove(amenity);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> AmenityNameExistsAsync(string name, long? excludeId = null)
        {
            var query = _context.Amenities.Where(a => a.Name == name);

            if (excludeId.HasValue)
            {
                query = query.Where(a => a.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }
    }
}
