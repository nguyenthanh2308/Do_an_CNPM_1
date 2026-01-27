using HotelManagement.API.Exceptions;
using HotelManagement.API.Models.DTOs.Common;
using HotelManagement.API.Models.Entities;
using HotelManagement.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HotelsController : ControllerBase
    {
        private readonly IHotelService _hotelService;
        private readonly ILogger<HotelsController> _logger;

        public HotelsController(IHotelService hotelService, ILogger<HotelsController> logger)
        {
            _hotelService = hotelService;
            _logger = logger;
        }

        /// <summary>
        /// Get all hotels
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllHotels()
        {
            try
            {
                var hotels = await _hotelService.GetAllHotelsAsync();
                return Ok(new ApiResponse<IEnumerable<Hotel>>
                {
                    Success = true,
                    Data = hotels
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get hotels");
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "An error occurred while retrieving hotels"
                });
            }
        }

        /// <summary>
        /// Get hotel by ID
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetHotelById(long id)
        {
            try
            {
                var hotel = await _hotelService.GetHotelByIdAsync(id);
                if (hotel == null)
                {
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = $"Hotel with ID {id} not found"
                    });
                }

                return Ok(new ApiResponse<Hotel>
                {
                    Success = true,
                    Data = hotel
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get hotel {HotelId}", id);
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "An error occurred while retrieving hotel"
                });
            }
        }

        /// <summary>
        /// Get hotel with all details (rooms, room types)
        /// </summary>
        [HttpGet("{id}/details")]
        [AllowAnonymous]
        public async Task<IActionResult> GetHotelWithDetails(long id)
        {
            try
            {
                var hotel = await _hotelService.GetHotelWithDetailsAsync(id);
                if (hotel == null)
                {
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = $"Hotel with ID {id} not found"
                    });
                }

                return Ok(new ApiResponse<Hotel>
                {
                    Success = true,
                    Data = hotel
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get hotel details {HotelId}", id);
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "An error occurred while retrieving hotel details"
                });
            }
        }

        /// <summary>
        /// Create new hotel (Manager only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> CreateHotel([FromBody] Hotel hotel)
        {
            try
            {
                var createdHotel = await _hotelService.CreateHotelAsync(hotel);
                return CreatedAtAction(nameof(GetHotelById), new { id = createdHotel.Id }, new ApiResponse<Hotel>
                {
                    Success = true,
                    Message = "Hotel created successfully",
                    Data = createdHotel
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
                _logger.LogError(ex, "Failed to create hotel");
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "An error occurred while creating hotel"
                });
            }
        }

        /// <summary>
        /// Update hotel (Manager only)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> UpdateHotel(long id, [FromBody] Hotel hotel)
        {
            try
            {
                var updatedHotel = await _hotelService.UpdateHotelAsync(id, hotel);
                return Ok(new ApiResponse<Hotel>
                {
                    Success = true,
                    Message = "Hotel updated successfully",
                    Data = updatedHotel
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
                _logger.LogError(ex, "Failed to update hotel {HotelId}", id);
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "An error occurred while updating hotel"
                });
            }
        }

        /// <summary>
        /// Delete hotel (Manager only)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> DeleteHotel(long id)
        {
            try
            {
                var success = await _hotelService.DeleteHotelAsync(id);
                if (success)
                {
                    return Ok(new ApiResponse
                    {
                        Success = true,
                        Message = "Hotel deleted successfully"
                    });
                }
                else
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "Failed to delete hotel"
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
                _logger.LogError(ex, "Failed to delete hotel {HotelId}", id);
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "An error occurred while deleting hotel"
                });
            }
        }
    }
}
