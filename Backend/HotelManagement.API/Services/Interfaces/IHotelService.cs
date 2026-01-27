using HotelManagement.API.Models.DTOs.Hotel;

namespace HotelManagement.API.Services.Interfaces;

public interface IHotelService
{
    Task<IEnumerable<HotelDto>> GetAllHotelsAsync();
    Task<HotelDto> GetHotelByIdAsync(long id);
    Task<HotelDto> CreateHotelAsync(CreateHotelDto dto);
    Task<HotelDto> UpdateHotelAsync(long id, UpdateHotelDto dto);
    Task<bool> DeleteHotelAsync(long id);
    Task<HotelDto> GetHotelWithDetailsAsync(long id);
}
