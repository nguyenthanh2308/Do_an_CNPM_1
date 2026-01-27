using HotelManagement.API.Exceptions;
using HotelManagement.API.Models.DTOs.Common;
using HotelManagement.API.Models.DTOs.Guest;
using HotelManagement.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Receptionist,Manager,Admin")]
    public class GuestsController : ControllerBase
    {
        private readonly IGuestService _guestService;
        private readonly ILogger<GuestsController> _logger;

        public GuestsController(IGuestService guestService, ILogger<GuestsController> logger)
        {
            _guestService = guestService;
            _logger = logger;
        }

        /// <summary>
        /// Get all guests with pagination
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllGuests([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var guests = await _guestService.GetAllGuestsAsync(page, pageSize);
                return Ok(new ApiResponse<PaginatedResponse<GuestDto>>
                {
                    Success = true,
                    Data = guests
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get guests");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving guests"
                });
            }
        }

        /// <summary>
        /// Get guest by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetGuestById(long id)
        {
            try
            {
                var guest = await _guestService.GetGuestByIdAsync(id);
                if (guest == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Guest with ID {id} not found"
                    });
                }

                return Ok(new ApiResponse<GuestDto>
                {
                    Success = true,
                    Data = guest
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get guest {GuestId}", id);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving guest"
                });
            }
        }

        /// <summary>
        /// Search guests by identity number (CCCD/CMND), email, or name
        /// </summary>
        [HttpGet("search")]
        public async Task<IActionResult> SearchGuests([FromQuery] string? identity, [FromQuery] string? email)
        {
            try
            {
                GuestDto? guest = null;

                if (!string.IsNullOrEmpty(identity))
                {
                    guest = await _guestService.GetGuestByIdentityNumberAsync(identity);
                }
                else if (!string.IsNullOrEmpty(email))
                {
                    guest = await _guestService.GetGuestByEmailAsync(email);
                }
                else
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Please provide at least one search parameter (identity or email)"
                    });
                }

                if (guest == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Guest not found"
                    });
                }

                return Ok(new ApiResponse<GuestDto>
                {
                    Success = true,
                    Data = guest
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to search guests");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while searching guests"
                });
            }
        }

        /// <summary>
        /// Create new guest
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateGuest([FromBody] CreateGuestDto dto)
        {
            try
            {
                var guest = await _guestService.CreateGuestAsync(dto);
                return CreatedAtAction(nameof(GetGuestById), new { id = guest.Id }, new ApiResponse<GuestDto>
                {
                    Success = true,
                    Message = "Guest created successfully",
                    Data = guest
                });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message,
                    Errors = ex.Errors
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create guest");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while creating guest"
                });
            }
        }

        /// <summary>
        /// Update guest information
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateGuest(long id, [FromBody] CreateGuestDto dto)
        {
            try
            {
                var guest = await _guestService.UpdateGuestAsync(id, dto);
                return Ok(new ApiResponse<GuestDto>
                {
                    Success = true,
                    Message = "Guest updated successfully",
                    Data = guest
                });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message,
                    Errors = ex.Errors
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update guest {GuestId}", id);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while updating guest"
                });
            }
        }

        /// <summary>
        /// Delete guest (Manager only)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> DeleteGuest(long id)
        {
            try
            {
                var success = await _guestService.DeleteGuestAsync(id);
                if (success)
                {
                    return Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "Guest deleted successfully"
                    });
                }
                else
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Failed to delete guest"
                    });
                }
            }
            catch (NotFoundException ex)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (BusinessException ex)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete guest {GuestId}", id);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while deleting guest"
                });
            }
        }
    }
}
