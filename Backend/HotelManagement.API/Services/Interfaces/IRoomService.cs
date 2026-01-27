using HotelManagement.API.Models.DTOs.Room;
using HotelManagement.API.Models.DTOs.Common;

namespace HotelManagement.API.Services.Interfaces;

public interface IRoomService
{
    Task<IEnumerable<RoomDto>> GetAvailableRoomsAsync(RoomAvailabilityDto dto);
    Task<RoomDto> GetRoomByIdAsync(long id);
    Task<PaginatedResponse<RoomDto>> GetRoomsByHotelAsync(long hotelId, int pageNumber = 1, int pageSize = 10);
    Task<RoomDto> CreateRoomAsync(CreateRoomDto dto);
    Task<RoomDto> UpdateRoomAsync(long id, CreateRoomDto dto);
    Task<bool> DeleteRoomAsync(long id);
    Task<bool> UpdateRoomStatusAsync(long id, string status);
}
