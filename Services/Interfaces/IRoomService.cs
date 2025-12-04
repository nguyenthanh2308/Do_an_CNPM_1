using System.Collections.Generic;
using System.Threading.Tasks;
using HotelManagementSystem.Models.Entities;

namespace HotelManagementSystem.Services.Interfaces
{
    public interface IRoomService
    {
        /// <summary>
        /// Lấy danh sách tất cả phòng (có thể filter theo hotelId / roomTypeId nếu cần).
        /// </summary>
        Task<List<Room>> GetAllAsync(long? hotelId = null, long? roomTypeId = null);

        /// <summary>
        /// Tìm phòng theo Id.
        /// </summary>
        Task<Room?> GetByIdAsync(long id);

        /// <summary>
        /// Thêm mới 1 phòng.
        /// </summary>
        Task<Room> CreateAsync(Room room);

        /// <summary>
        /// Cập nhật trạng thái phòng (Vacant/Occupied/Cleaning/Maintenance).
        /// </summary>
        Task<bool> UpdateStatusAsync(long roomId, string newStatus);

        Task<Room?> UpdateAsync(Room room);

        /// <summary>
        /// Xóa phòng.
        /// </summary>
        Task<bool> DeleteAsync(long id);

        // Hàm tìm phòng trống
        Task<List<Room>> SearchAvailableRoomsAsync(
            DateTime checkInDate,
            DateTime checkOutDate,
            int numberOfGuests,
            long? hotelId = null,
            long? roomTypeId = null);
    }
}
