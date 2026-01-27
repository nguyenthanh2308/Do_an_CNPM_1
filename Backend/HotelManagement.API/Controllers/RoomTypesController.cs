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
    public class RoomTypesController : ControllerBase
    {
        private readonly IRoomTypeService _roomTypeService;
        private readonly ILogger<RoomTypesController> _logger;

        public RoomTypesController(IRoomTypeService roomTypeService, ILogger<RoomTypesController> logger)
        {
            _roomTypeService = roomTypeService;
            _logger = logger;
        }

        /// <summary>
        /// Get all room types
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllRoomTypes()
        {
            try
            {
                var roomTypes = await _roomTypeService.GetAllRoomTypesAsync();
                return Ok(new ApiResponse<IEnumerable<RoomType>>
                {
                    Success = true,
                    Data = roomTypes
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get room types");
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "An error occurred while retrieving room types"
                });
            }
        }

        /// <summary>
        /// Get room type by ID
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetRoomTypeById(long id)
        {
            try
            {
                var roomType = await _roomTypeService.GetRoomTypeByIdAsync(id);
                if (roomType == null)
                {
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = $"Room type with ID {id} not found"
                    });
                }

                return Ok(new ApiResponse<RoomType>
                {
                    Success = true,
                    Data = roomType
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get room type {RoomTypeId}", id);
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "An error occurred while retrieving room type"
                });
            }
        }

        /// <summary>
        /// Get room type with amenities
        /// </summary>
        [HttpGet("{id}/amenities")]
        [AllowAnonymous]
        public async Task<IActionResult> GetRoomTypeWithAmenities(long id)
        {
            try
            {
                var roomType = await _roomTypeService.GetRoomTypeWithAmenitiesAsync(id);
                if (roomType == null)
                {
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = $"Room type with ID {id} not found"
                    });
                }

                return Ok(new ApiResponse<RoomType>
                {
                    Success = true,
                    Data = roomType
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get room type with amenities {RoomTypeId}", id);
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "An error occurred while retrieving room type"
                });
            }
        }

        /// <summary>
        /// Get room types by hotel
        /// </summary>
        [HttpGet("hotel/{hotelId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetRoomTypesByHotel(long hotelId)
        {
            try
            {
                var roomTypes = await _roomTypeService.GetRoomTypesByHotelAsync(hotelId);
                return Ok(new ApiResponse<IEnumerable<RoomType>>
                {
                    Success = true,
                    Data = roomTypes
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get room types for hotel {HotelId}", hotelId);
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "An error occurred while retrieving room types"
                });
            }
        }

        /// <summary>
        /// Create new room type (Manager only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> CreateRoomType([FromBody] RoomType roomType)
        {
            try
            {
                var createdRoomType = await _roomTypeService.CreateRoomTypeAsync(roomType);
                return CreatedAtAction(nameof(GetRoomTypeById), new { id = createdRoomType.Id }, new ApiResponse<RoomType>
                {
                    Success = true,
                    Message = "Room type created successfully",
                    Data = createdRoomType
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
            catch (NotFoundException ex)
            {
                return NotFound(new ApiResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create room type");
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "An error occurred while creating room type"
                });
            }
        }

        /// <summary>
        /// Update room type (Manager only)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> UpdateRoomType(long id, [FromBody] RoomType roomType)
        {
            try
            {
                var updatedRoomType = await _roomTypeService.UpdateRoomTypeAsync(id, roomType);
                return Ok(new ApiResponse<RoomType>
                {
                    Success = true,
                    Message = "Room type updated successfully",
                    Data = updatedRoomType
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
                _logger.LogError(ex, "Failed to update room type {RoomTypeId}", id);
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "An error occurred while updating room type"
                });
            }
        }

        /// <summary>
        /// Delete room type (Manager only)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> DeleteRoomType(long id)
        {
            try
            {
                var success = await _roomTypeService.DeleteRoomTypeAsync(id);
                if (success)
                {
                    return Ok(new ApiResponse
                    {
                        Success = true,
                        Message = "Room type deleted successfully"
                    });
                }
                else
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "Failed to delete room type"
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
                _logger.LogError(ex, "Failed to delete room type {RoomTypeId}", id);
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "An error occurred while deleting room type"
                });
            }
        }
    }
}
