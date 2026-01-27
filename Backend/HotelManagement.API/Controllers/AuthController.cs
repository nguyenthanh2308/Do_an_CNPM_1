using HotelManagement.API.Exceptions;
using HotelManagement.API.Models.DTOs.Auth;
using HotelManagement.API.Models.DTOs.Common;
using HotelManagement.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HotelManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Login with username and password
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            try
            {
                var response = await _authService.LoginAsync(request);
                return Ok(new ApiResponse<LoginResponseDto>
                {
                    Success = true,
                    Message = "Login successful",
                    Data = response
                });
            }
            catch (UnauthorizedException ex)
            {
                return Unauthorized(new ApiResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login failed for username: {Username}", request.Username);
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "An error occurred during login"
                });
            }
        }

        /// <summary>
        /// Register new customer account
        /// </summary>
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            try
            {
                var response = await _authService.RegisterAsync(request);
                return Ok(new ApiResponse<LoginResponseDto>
                {
                    Success = true,
                    Message = "Registration successful",
                    Data = response
                });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = ex.Message,
                    Errors = ex.Errors
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Registration failed for username: {Username}", request.Username);
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "An error occurred during registration"
                });
            }
        }

        /// <summary>
        /// Refresh JWT token using refresh token
        /// </summary>
        [HttpPost("refresh-token")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request)
        {
            try
            {
                var response = await _authService.RefreshTokenAsync(request.RefreshToken);
                return Ok(new ApiResponse<LoginResponseDto>
                {
                    Success = true,
                    Message = "Token refreshed successfully",
                    Data = response
                });
            }
            catch (UnauthorizedException ex)
            {
                return Unauthorized(new ApiResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Token refresh failed");
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "An error occurred during token refresh"
                });
            }
        }

        /// <summary>
        /// Get current user information
        /// </summary>
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out long userId))
                {
                    return Unauthorized(new ApiResponse
                    {
                        Success = false,
                        Message = "Invalid user token"
                    });
                }

                var user = await _authService.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = "User not found"
                    });
                }

                return Ok(new ApiResponse<UserDto>
                {
                    Success = true,
                    Data = user
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get current user");
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "An error occurred while retrieving user information"
                });
            }
        }

        /// <summary>
        /// Logout - invalidate refresh token
        /// </summary>
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout([FromBody] RefreshTokenRequestDto request)
        {
            try
            {
                var success = await _authService.RevokeTokenAsync(request.RefreshToken);
                if (success)
                {
                    return Ok(new ApiResponse
                    {
                        Success = true,
                        Message = "Logout successful"
                    });
                }
                else
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "Invalid refresh token"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Logout failed");
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "An error occurred during logout"
                });
            }
        }
    }
}
