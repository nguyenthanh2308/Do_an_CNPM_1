using System.Collections.Generic;
using System.Threading.Tasks;
using HotelManagementSystem.Models.Entities;
using HotelManagementSystem.Models.ViewModels.RoomType;
using Microsoft.AspNetCore.Http;

namespace HotelManagementSystem.Services.Interfaces
{
    public interface IRoomTypeService
    {
        /// <summary>
        /// Lấy danh sách tất cả loại phòng
        /// </summary>
        Task<List<RoomTypeViewModel>> GetAllAsync(long? hotelId = null);

        /// <summary>
        /// Lấy loại phòng theo Id
        /// </summary>
        Task<RoomTypeViewModel?> GetByIdAsync(long id);

        /// <summary>
        /// Lấy entity RoomType theo Id
        /// </summary>
        Task<RoomType?> GetRoomTypeEntityByIdAsync(long id);

        /// <summary>
        /// Tạo loại phòng mới
        /// </summary>
        Task<RoomType> CreateAsync(RoomTypeViewModel model);

        /// <summary>
        /// Cập nhật loại phòng
        /// </summary>
        Task<RoomType?> UpdateAsync(RoomTypeViewModel model);

        /// <summary>
        /// Xóa loại phòng
        /// </summary>
        Task<bool> DeleteAsync(long id);

        /// <summary>
        /// Upload hình ảnh cho loại phòng
        /// </summary>
        Task<string> UploadImageAsync(IFormFile imageFile);

        /// <summary>
        /// Cập nhật amenities cho loại phòng
        /// </summary>
        Task UpdateAmenitiesAsync(long roomTypeId, List<long> amenityIds);

        /// <summary>
        /// Kiểm tra tên loại phòng đã tồn tại trong khách sạn chưa
        /// </summary>
        Task<bool> RoomTypeNameExistsAsync(long hotelId, string name, long? excludeId = null);
    }
}
