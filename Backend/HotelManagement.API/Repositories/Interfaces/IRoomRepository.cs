using HotelManagement.API.Models.Entities;

namespace HotelManagement.API.Repositories.Interfaces
{
    public interface IRoomRepository : IRepository<Room>
    {
        Task<Room?> GetRoomWithDetailsAsync(long roomId);
        Task<IEnumerable<Room>> GetRoomsByHotelAsync(long hotelId);
        Task<IEnumerable<Room>> GetRoomsByTypeAsync(long roomTypeId);
        Task<IEnumerable<Room>> GetAvailableRoomsAsync(long hotelId, DateTime checkIn, DateTime checkOut, long? roomTypeId = null);
        Task<IEnumerable<Room>> GetRoomsByStatusAsync(string status);
        Task<Room?> GetRoomByNumberAsync(long hotelId, string roomNumber);
    }
}
