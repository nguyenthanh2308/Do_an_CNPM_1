using HotelManagement.API.Models.DTOs.Guest;
using HotelManagement.API.Models.DTOs.Common;

namespace HotelManagement.API.Services.Interfaces;

public interface IGuestService
{
    Task<GuestDto> GetGuestByIdAsync(long id);
    Task<GuestDto?> GetGuestByEmailAsync(string email);
    Task<GuestDto?> GetGuestByIdentityNumberAsync(string idNumber);
    Task<PaginatedResponse<GuestDto>> GetAllGuestsAsync(int pageNumber = 1, int pageSize = 10);
    Task<GuestDto> CreateGuestAsync(CreateGuestDto dto);
    Task<GuestDto> UpdateGuestAsync(long id, CreateGuestDto dto);
    Task<bool> DeleteGuestAsync(long id);
}
