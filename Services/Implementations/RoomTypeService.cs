using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HotelManagementSystem.Data;
using HotelManagementSystem.Models.Entities;
using HotelManagementSystem.Models.ViewModels.RoomType;
using HotelManagementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace HotelManagementSystem.Services.Implementations
{
    public class RoomTypeService : IRoomTypeService
    {
        private readonly HotelDbContext _context;

        public RoomTypeService(HotelDbContext context)
        {
            _context = context;
        }

        public async Task<List<RoomTypeViewModel>> GetAllAsync(long? hotelId = null)
        {
            var query = _context.RoomTypes
                .Include(rt => rt.Hotel)
                .Include(rt => rt.Rooms)
                .Include(rt => rt.RatePlans)
                .Include(rt => rt.RoomTypeAmenities)
                    .ThenInclude(rta => rta.Amenity)
                .AsQueryable();

            if (hotelId.HasValue)
            {
                query = query.Where(rt => rt.HotelId == hotelId.Value);
            }

            var roomTypes = await query
                .OrderBy(rt => rt.Hotel.Name)
                .ThenBy(rt => rt.Name)
                .ToListAsync();

            return roomTypes.Select(rt => new RoomTypeViewModel
            {
                Id = rt.Id,
                HotelId = rt.HotelId,
                HotelName = rt.Hotel.Name,
                Name = rt.Name,
                Capacity = rt.Capacity,
                BasePrice = rt.BasePrice,
                Description = rt.Description,
                DefaultImageUrl = rt.DefaultImageUrl,
                CreatedAt = rt.CreatedAt,
                RoomCount = rt.Rooms.Count,
                RatePlanCount = rt.RatePlans.Count,
                SelectedAmenityIds = rt.RoomTypeAmenities.Select(rta => rta.AmenityId).ToList(),
                AmenityNames = rt.RoomTypeAmenities.Select(rta => rta.Amenity.Name).ToList()
            }).ToList();
        }

        public async Task<RoomTypeViewModel?> GetByIdAsync(long id)
        {
            var roomType = await _context.RoomTypes
                .Include(rt => rt.Hotel)
                .Include(rt => rt.Rooms)
                .Include(rt => rt.RatePlans)
                .Include(rt => rt.RoomTypeAmenities)
                    .ThenInclude(rta => rta.Amenity)
                .FirstOrDefaultAsync(rt => rt.Id == id);

            if (roomType == null)
            {
                return null;
            }

            // Load all amenities for checkbox list
            var allAmenities = await _context.Amenities
                .OrderBy(a => a.Name)
                .ToListAsync();

            var selectedAmenityIds = roomType.RoomTypeAmenities.Select(rta => rta.AmenityId).ToList();

            return new RoomTypeViewModel
            {
                Id = roomType.Id,
                HotelId = roomType.HotelId,
                HotelName = roomType.Hotel.Name,
                Name = roomType.Name,
                Capacity = roomType.Capacity,
                BasePrice = roomType.BasePrice,
                Description = roomType.Description,
                DefaultImageUrl = roomType.DefaultImageUrl,
                CreatedAt = roomType.CreatedAt,
                RoomCount = roomType.Rooms.Count,
                RatePlanCount = roomType.RatePlans.Count,
                SelectedAmenityIds = selectedAmenityIds,
                Amenities = allAmenities.Select(a => new Models.ViewModels.Amenity.AmenityViewModel
                {
                    Id = a.Id,
                    Name = a.Name
                }).ToList(),
                AmenityNames = roomType.RoomTypeAmenities.Select(rta => rta.Amenity.Name).ToList()
            };
        }

        public async Task<RoomType?> GetRoomTypeEntityByIdAsync(long id)
        {
            return await _context.RoomTypes.FindAsync(id);
        }

        public async Task<RoomType> CreateAsync(RoomTypeViewModel model)
        {
            // Kiểm tra tên loại phòng đã tồn tại
            if (await RoomTypeNameExistsAsync(model.HotelId, model.Name))
            {
                throw new InvalidOperationException($"Loại phòng '{model.Name}' đã tồn tại trong khách sạn này");
            }

            // Xử lý upload hình ảnh nếu có
            if (model.ImageFile != null)
            {
                model.DefaultImageUrl = await UploadImageAsync(model.ImageFile);
            }

            var roomType = new RoomType
            {
                HotelId = model.HotelId,
                Name = model.Name,
                Capacity = model.Capacity,
                BasePrice = model.BasePrice,
                Description = model.Description,
                DefaultImageUrl = model.DefaultImageUrl,
                CreatedAt = DateTime.Now
            };

            _context.RoomTypes.Add(roomType);
            await _context.SaveChangesAsync();

            // Cập nhật amenities
            if (model.SelectedAmenityIds != null && model.SelectedAmenityIds.Any())
            {
                await UpdateAmenitiesAsync(roomType.Id, model.SelectedAmenityIds);
            }

            return roomType;
        }

        public async Task<RoomType?> UpdateAsync(RoomTypeViewModel model)
        {
            var roomType = await _context.RoomTypes.FindAsync(model.Id);
            if (roomType == null)
            {
                return null;
            }

            // Kiểm tra tên loại phòng đã tồn tại
            if (await RoomTypeNameExistsAsync(model.HotelId, model.Name, model.Id))
            {
                throw new InvalidOperationException($"Loại phòng '{model.Name}' đã tồn tại trong khách sạn này");
            }

            // Xử lý upload hình ảnh mới nếu có
            if (model.ImageFile != null)
            {
                model.DefaultImageUrl = await UploadImageAsync(model.ImageFile);
            }

            roomType.HotelId = model.HotelId;
            roomType.Name = model.Name;
            roomType.Capacity = model.Capacity;
            roomType.BasePrice = model.BasePrice;
            roomType.Description = model.Description;
            
            if (!string.IsNullOrEmpty(model.DefaultImageUrl))
            {
                roomType.DefaultImageUrl = model.DefaultImageUrl;
            }

            await _context.SaveChangesAsync();

            // Cập nhật amenities
            await UpdateAmenitiesAsync(roomType.Id, model.SelectedAmenityIds ?? new List<long>());

            return roomType;
        }

        public async Task<bool> DeleteAsync(long id)
        {
            var roomType = await _context.RoomTypes
                .Include(rt => rt.Rooms)
                .Include(rt => rt.RatePlans)
                .FirstOrDefaultAsync(rt => rt.Id == id);

            if (roomType == null)
            {
                return false;
            }

            if (roomType.Rooms.Any())
            {
                throw new InvalidOperationException("Không thể xóa loại phòng đang có phòng. Vui lòng xóa tất cả phòng trước.");
            }

            if (roomType.RatePlans.Any())
            {
                throw new InvalidOperationException("Không thể xóa loại phòng đang có rate plan. Vui lòng xóa tất cả rate plan trước.");
            }

            _context.RoomTypes.Remove(roomType);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<string> UploadImageAsync(IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                throw new ArgumentException("File không hợp lệ");
            }

            // Validate file type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
            
            if (!allowedExtensions.Contains(extension))
            {
                throw new ArgumentException("Chỉ chấp nhận file ảnh (.jpg, .jpeg, .png, .gif)");
            }

            // Validate file size (max 5MB)
            if (imageFile.Length > 5 * 1024 * 1024)
            {
                throw new ArgumentException("Kích thước file không được vượt quá 5MB");
            }

            // Generate unique filename
            var fileName = $"{Guid.NewGuid()}{extension}";
            var uploadsFolder = Path.Combine("wwwroot", "images", "rooms");
            
            // Create directory if not exists
            Directory.CreateDirectory(uploadsFolder);
            
            var filePath = Path.Combine(uploadsFolder, fileName);

            // Save file
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }

            // Return relative URL
            return $"/images/rooms/{fileName}";
        }

        public async Task UpdateAmenitiesAsync(long roomTypeId, List<long> amenityIds)
        {
            // Xóa tất cả amenities hiện tại
            var existingAmenities = await _context.RoomTypeAmenities
                .Where(rta => rta.RoomTypeId == roomTypeId)
                .ToListAsync();

            _context.RoomTypeAmenities.RemoveRange(existingAmenities);

            // Thêm amenities mới
            if (amenityIds != null && amenityIds.Any())
            {
                var newAmenities = amenityIds.Select(amenityId => new RoomTypeAmenity
                {
                    RoomTypeId = roomTypeId,
                    AmenityId = amenityId
                }).ToList();

                await _context.RoomTypeAmenities.AddRangeAsync(newAmenities);
            }

            await _context.SaveChangesAsync();
        }

        public async Task<bool> RoomTypeNameExistsAsync(long hotelId, string name, long? excludeId = null)
        {
            var query = _context.RoomTypes
                .Where(rt => rt.HotelId == hotelId && rt.Name == name);

            if (excludeId.HasValue)
            {
                query = query.Where(rt => rt.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }
    }
}
