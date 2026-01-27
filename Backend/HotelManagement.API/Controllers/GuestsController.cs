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
                var guests = await _guestService.GetAllGuestsAsync();
                var pagedGuests = guests.Skip((page - 1) * pageSize).Take(pageSize);
                
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = new
                    {
                        Items = pagedGuests,
                        TotalCount = guests.Count(),
                        Page = page,
                        PageSize = pageSize,
                        TotalPages = (int)Math.Ceiling(guests.Count() / (double)pageSize)
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get guests");
                return StatusCode(500, new ApiResponse
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
                    return NotFound(new ApiResponse
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
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "An error occurred while retrieving guest"
                });
            }
        }

        /// <summary>
        /// Search guests by identity number (CCCD/CMND)
        /// </summary>
        [HttpGet("search")]
        public async Task<IActionResult> SearchGuests([FromQuery] string? identity, [FromQuery] string? name, [FromQuery] string? email)
        {
            try
            {
                IEnumerable<GuestDto> guests;

                if (!string.IsNullOrEmpty(identity))
                {
                    var guest = await _guestService.GetGuestByIdentityNumberAsync(identity);
                    guests = guest != null ? new[] { guest } : Array.Empty<GuestDto>();
                }
                else if (!string.IsNullOrEmpty(email))
                {
                    var guest = await _guestService.GetGuestByEmailAsync(email);
                    guests = guest != null ? new[] { guest } : Array.Empty<GuestDto>();
                }
                else if (!string.IsNullOrEmpty(name))
                {
                    var allGuests = await _guestService.GetAllGuestsAsync();
                    guests = allGuests.Where(g => 
                        (g.FirstName + " " + g.LastName).Contains(name, StringComparison.OrdinalIgnoreCase));
                }
                else
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "Please provide at least one search parameter (identity, name, or email)"
                    });
                }

                return Ok(new ApiResponse<IEnumerable<GuestDto>>
                {
                    Success = true,
                    Data = guests
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to search guests");
                return StatusCode(500, new ApiResponse
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
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = ex.Message,
                    Errors = ex.Errors
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create guest");
                return StatusCode(500, new ApiResponse
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
        public async Task<IActionResult> UpdateGuest(long id, [FromBody] UpdateGuestDto dto)
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
                return NotFound(new ApiResponse
                {
                    Success = false,
                    Message = ex.Message
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
                _logger.LogError(ex, "Failed to update guest {GuestId}", id);
                return StatusCode(500, new ApiResponse
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
                    return Ok(new ApiResponse
                    {
                        Success = true,
                        Message = "Guest deleted successfully"
                    });
                }
                else
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "Failed to delete guest"
                    });
                }
            }
            catch (NotFoundException ex)
            {
                return NotFound(new ApiResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (BusinessException ex)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete guest {GuestId}", id);
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "An error occurred while deleting guest"
                });
            }
        }
    }
}
