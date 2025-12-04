using System.Collections.Generic;
using System.Threading.Tasks;
using HotelManagementSystem.Models.Entities;
using HotelManagementSystem.Models.ViewModels.Hotel;

namespace HotelManagementSystem.Services.Interfaces
{
    public interface IHotelService
    {
        /// <summary>
        /// Lấy danh sách tất cả khách sạn
        /// </summary>
        Task<List<HotelViewModel>> GetAllAsync();

        /// <summary>
        /// Lấy thông tin chi tiết khách sạn theo Id
        /// </summary>
        Task<HotelViewModel?> GetByIdAsync(long id);

        /// <summary>
        /// Lấy entity Hotel theo Id
        /// </summary>
        Task<Hotel?> GetHotelEntityByIdAsync(long id);

        /// <summary>
        /// Tạo khách sạn mới
        /// </summary>
        Task<Hotel> CreateAsync(HotelViewModel model);

        /// <summary>
        /// Cập nhật thông tin khách sạn
        /// </summary>
        Task<Hotel?> UpdateAsync(HotelViewModel model);

        /// <summary>
        /// Xóa khách sạn
        /// </summary>
        Task<bool> DeleteAsync(long id);

        /// <summary>
        /// Kiểm tra xem tên khách sạn đã tồn tại chưa
        /// </summary>
        Task<bool> HotelNameExistsAsync(string name, long? excludeId = null);
    }
}
