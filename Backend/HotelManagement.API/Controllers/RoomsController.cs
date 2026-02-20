using HotelManagement.API.Exceptions;
using HotelManagement.API.Models.DTOs.Common;
using HotelManagement.API.Models.DTOs.Room;
using HotelManagement.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoomsController : ControllerBase
    {
        private readonly IRoomService _roomService;
        private readonly ILogger<RoomsController> _logger;

        public RoomsController(IRoomService roomService, ILogger<RoomsController> logger)
        {
            _roomService = roomService;
            _logger = logger;
        }

        /// <summary>
        /// Get rooms by hotel with pagination
        /// </summary>
        [HttpGet("hotel/{hotelId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetRoomsByHotel(long hotelId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var rooms = await _roomService.GetRoomsByHotelAsync(hotelId, page, pageSize);
                return Ok(new ApiResponse<PaginatedResponse<RoomDto>>
                {
                    Success = true,
                    Data = rooms
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get rooms for hotel {HotelId}", hotelId);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving rooms"
                });
            }
        }

        /// <summary>
        /// Get available rooms by date range
        /// </summary>
        [HttpGet("available")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAvailableRooms([FromQuery] long hotelId, [FromQuery] DateTime checkIn, [FromQuery] DateTime checkOut)
        {
            try
            {
                if (checkIn < DateTime.Today)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Check-in date cannot be in the past"
                    });
                }

                if (checkOut <= checkIn)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Check-out date must be after check-in date"
                    });
                }

                var dto = new RoomAvailabilityDto
                {
                    HotelId = hotelId,
                    CheckInDate = checkIn,
                    CheckOutDate = checkOut
                };

                var rooms = await _roomService.GetAvailableRoomsAsync(dto);
                return Ok(new ApiResponse<IEnumerable<RoomDto>>
                {
                    Success = true,
                    Data = rooms
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get available rooms");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving available rooms"
                });
            }
        }

        /// <summary>
        /// Get room by ID
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetRoomById(long id)
        {
            try
            {
                var room = await _roomService.GetRoomByIdAsync(id);
                if (room == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Room with ID {id} not found"
                    });
                }

                return Ok(new ApiResponse<RoomDto>
                {
                    Success = true,
                    Data = room
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get room {RoomId}", id);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving room"
                });
            }
        }

        /// <summary>
        /// Update room status (Receptionist, Manager)
        /// </summary>
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Receptionist,Manager,Admin")]
        public async Task<IActionResult> UpdateRoomStatus(long id, [FromBody] UpdateRoomStatusDto dto)
        {
            try
            {
                var validStatuses = new[] { "Available", "Occupied", "Maintenance", "Cleaning", "Reserved", "OutOfService" };
                if (!validStatuses.Contains(dto.Status))
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Invalid status. Must be one of: {string.Join(", ", validStatuses)}"
                    });
                }

                var success = await _roomService.UpdateRoomStatusAsync(id, dto.Status);
                if (success)
                {
                    return Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "Room status updated successfully"
                    });
                }
                else
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Failed to update room status"
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update room status {RoomId}", id);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while updating room status"
                });
            }
        }

        /// <summary>
        /// Create new room (Manager only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> CreateRoom([FromBody] CreateRoomDto dto)
        {
            try
            {
                var room = await _roomService.CreateRoomAsync(dto);
                return CreatedAtAction(nameof(GetRoomById), new { id = room.Id }, new ApiResponse<RoomDto>
                {
                    Success = true,
                    Message = "Room created successfully",
                    Data = room
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
            catch (NotFoundException ex)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create room");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while creating room"
                });
            }
        }

        /// <summary>
        /// Update room (Manager only)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> UpdateRoom(long id, [FromBody] CreateRoomDto dto)
        {
            try
            {
                var room = await _roomService.UpdateRoomAsync(id, dto);
                return Ok(new ApiResponse<RoomDto>
                {
                    Success = true,
                    Message = "Room updated successfully",
                    Data = room
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
                _logger.LogError(ex, "Failed to update room {RoomId}", id);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while updating room"
                });
            }
        }

        /// <summary>
        /// Delete room (Manager only)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> DeleteRoom(long id)
        {
            try
            {
                var success = await _roomService.DeleteRoomAsync(id);
                if (success)
                {
                    return Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "Room deleted successfully"
                    });
                }
                else
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Failed to delete room"
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
                _logger.LogError(ex, "Failed to delete room {RoomId}", id);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while deleting room"
                });
            }
        }
    }
}
