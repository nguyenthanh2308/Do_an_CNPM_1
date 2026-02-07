using HotelManagement.API.Exceptions;
using HotelManagement.API.Models.DTOs.Amenity;
using HotelManagement.API.Models.DTOs.Common;
using HotelManagement.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AmenitiesController : ControllerBase
    {
        private readonly IAmenityService _amenityService;
        private readonly ILogger<AmenitiesController> _logger;

        public AmenitiesController(IAmenityService amenityService, ILogger<AmenitiesController> logger)
        {
            _amenityService = amenityService;
            _logger = logger;
        }

        /// <summary>
        /// Get all amenities
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllAmenities()
        {
            try
            {
                var amenities = await _amenityService.GetAllAmenitiesAsync();
                return Ok(new ApiResponse<IEnumerable<AmenityDto>>
                {
                    Success = true,
                    Data = amenities
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get amenities");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving amenities"
                });
            }
        }

        /// <summary>
        /// Get amenity by ID
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAmenityById(long id)
        {
            try
            {
                var amenity = await _amenityService.GetAmenityByIdAsync(id);
                if (amenity == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Amenity with ID {id} not found"
                    });
                }

                return Ok(new ApiResponse<AmenityDto>
                {
                    Success = true,
                    Data = amenity
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get amenity {AmenityId}", id);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving amenity"
                });
            }
        }

        /// <summary>
        /// Get amenities for a room type
        /// </summary>
        [HttpGet("roomtype/{roomTypeId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAmenitiesByRoomType(long roomTypeId)
        {
            try
            {
                var amenities = await _amenityService.GetAmenitiesByRoomTypeAsync(roomTypeId);
                return Ok(new ApiResponse<IEnumerable<AmenityDto>>
                {
                    Success = true,
                    Data = amenities
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get amenities for room type {RoomTypeId}", roomTypeId);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving amenities"
                });
            }
        }

        /// <summary>
        /// Create new amenity (Manager only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> CreateAmenity([FromBody] CreateAmenityDto dto)
        {
            try
            {
                var amenity = await _amenityService.CreateAmenityAsync(dto);
                return CreatedAtAction(nameof(GetAmenityById), new { id = amenity.Id }, new ApiResponse<AmenityDto>
                {
                    Success = true,
                    Message = "Amenity created successfully",
                    Data = amenity
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
                _logger.LogError(ex, "Failed to create amenity");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while creating amenity"
                });
            }
        }

        /// <summary>
        /// Update amenity (Manager only)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> UpdateAmenity(long id, [FromBody] CreateAmenityDto dto)
        {
            try
            {
                var amenity = await _amenityService.UpdateAmenityAsync(id, dto);
                return Ok(new ApiResponse<AmenityDto>
                {
                    Success = true,
                    Message = "Amenity updated successfully",
                    Data = amenity
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
                _logger.LogError(ex, "Failed to update amenity {AmenityId}", id);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while updating amenity"
                });
            }
        }

        /// <summary>
        /// Delete amenity (Manager only)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> DeleteAmenity(long id)
        {
            try
            {
                var success = await _amenityService.DeleteAmenityAsync(id);
                if (success)
                {
                    return Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "Amenity deleted successfully"
                    });
                }
                else
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Failed to delete amenity"
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
                _logger.LogError(ex, "Failed to delete amenity {AmenityId}", id);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while deleting amenity"
                });
            }
        }
    }
}
