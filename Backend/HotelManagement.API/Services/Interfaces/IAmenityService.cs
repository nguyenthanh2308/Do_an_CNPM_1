using HotelManagement.API.Models.DTOs.Amenity;

namespace HotelManagement.API.Services.Interfaces;

public interface IAmenityService
{
    Task<IEnumerable<AmenityDto>> GetAllAmenitiesAsync();
    Task<AmenityDto> GetAmenityByIdAsync(long id);
    Task<AmenityDto> CreateAmenityAsync(CreateAmenityDto dto);
    Task<AmenityDto> UpdateAmenityAsync(long id, CreateAmenityDto dto);
    Task<bool> DeleteAmenityAsync(long id);
    Task<IEnumerable<AmenityDto>> GetAmenitiesByRoomTypeAsync(long roomTypeId);
}
