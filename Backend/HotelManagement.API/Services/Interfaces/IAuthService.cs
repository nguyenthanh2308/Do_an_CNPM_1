using HotelManagement.API.Models.DTOs.Auth;

namespace HotelManagement.API.Services.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request);
    Task<LoginResponseDto> RegisterAsync(RegisterRequestDto request);
    Task<LoginResponseDto> RefreshTokenAsync(string refreshToken);
    Task<UserDto> GetCurrentUserAsync(long userId);
    Task LogoutAsync(long userId);
    Task<bool> ValidateTokenAsync(string token);
}
