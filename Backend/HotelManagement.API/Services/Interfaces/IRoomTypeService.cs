using HotelManagement.API.Models.DTOs.RoomType;

namespace HotelManagement.API.Services.Interfaces;

public interface IRoomTypeService
{
    Task<IEnumerable<RoomTypeDto>> GetAllRoomTypesAsync();
    Task<RoomTypeDto> GetRoomTypeByIdAsync(long id);
    Task<RoomTypeDto> CreateRoomTypeAsync(CreateRoomTypeDto dto);
    Task<RoomTypeDto> UpdateRoomTypeAsync(long id, UpdateRoomTypeDto dto);
    Task<bool> DeleteRoomTypeAsync(long id);
    Task<IEnumerable<RoomTypeDto>> GetRoomTypesByHotelAsync(long hotelId);
    Task<RoomTypeDto> GetRoomTypeWithAmenitiesAsync(long id);
}
