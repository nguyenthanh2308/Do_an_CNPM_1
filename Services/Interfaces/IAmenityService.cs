using System.Collections.Generic;
using System.Threading.Tasks;
using HotelManagementSystem.Models.Entities;
using HotelManagementSystem.Models.ViewModels.Amenity;

namespace HotelManagementSystem.Services.Interfaces
{
    public interface IAmenityService
    {
        /// <summary>
        /// Lấy danh sách tất cả tiện nghi
        /// </summary>
        Task<List<AmenityViewModel>> GetAllAsync();

        /// <summary>
        /// Lấy tiện nghi theo Id
        /// </summary>
        Task<AmenityViewModel?> GetByIdAsync(long id);

        /// <summary>
        /// Lấy entity Amenity theo Id
        /// </summary>
        Task<Amenity?> GetAmenityEntityByIdAsync(long id);

        /// <summary>
        /// Tạo tiện nghi mới
        /// </summary>
        Task<Amenity> CreateAsync(AmenityViewModel model);

        /// <summary>
        /// Cập nhật tiện nghi
        /// </summary>
        Task<Amenity?> UpdateAsync(AmenityViewModel model);

        /// <summary>
        /// Xóa tiện nghi
        /// </summary>
        Task<bool> DeleteAsync(long id);

        /// <summary>
        /// Kiểm tra tên tiện nghi đã tồn tại chưa
        /// </summary>
        Task<bool> AmenityNameExistsAsync(string name, long? excludeId = null);
    }
}
