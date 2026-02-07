using HotelManagement.API.Exceptions;
using HotelManagement.API.Models.DTOs.Booking;
using HotelManagement.API.Models.DTOs.Common;
using HotelManagement.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HotelManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingsController : ControllerBase
    {
        private readonly IBookingService _bookingService;
        private readonly ILogger<BookingsController> _logger;

        public BookingsController(IBookingService bookingService, ILogger<BookingsController> logger)
        {
            _bookingService = bookingService;
            _logger = logger;
        }

        /// <summary>
        /// Get all bookings with filters (Receptionist, Manager, Admin)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Receptionist,Manager,Admin")]
        public async Task<IActionResult> GetAllBookings([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var bookings = await _bookingService.GetAllBookingsAsync(page, pageSize);
                return Ok(new ApiResponse<PaginatedResponse<BookingDto>>
                {
                    Success = true,
                    Data = bookings
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get bookings");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving bookings"
                });
            }
        }

        /// <summary>
        /// Get my bookings (Customer)
        /// </summary>
        [HttpGet("my-bookings")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetMyBookings([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out long userId))
                {
                    return Unauthorized(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid user token"
                    });
                }

                // Get all bookings and filter by guest ID
                var allBookings = await _bookingService.GetAllBookingsAsync(1, 1000);
                var myBookings = allBookings.Items.Where(b => b.GuestId == userId).ToList();
                
                return Ok(new ApiResponse<IEnumerable<BookingDto>>
                {
                    Success = true,
                    Data = myBookings
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get user bookings");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving bookings"
                });
            }
        }

        /// <summary>
        /// Get booking by ID
        /// </summary>
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetBookingById(long id)
        {
            try
            {
                var booking = await _bookingService.GetBookingByIdAsync(id);
                if (booking == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Booking with ID {id} not found"
                    });
                }

                // Check if user is customer and owns this booking
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                if (userRole == "Customer")
                {
                    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (long.TryParse(userIdClaim, out long userId) && booking.Guest.Id != userId)
                    {
                        return Forbid();
                    }
                }

                return Ok(new ApiResponse<BookingDetailDto>
                {
                    Success = true,
                    Data = booking
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get booking {BookingId}", id);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving booking"
                });
            }
        }

        /// <summary>
        /// Create new booking
        /// </summary>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateBooking([FromBody] CreateBookingDto dto)
        {
            try
            {
                var booking = await _bookingService.CreateBookingAsync(dto);
                return CreatedAtAction(nameof(GetBookingById), new { id = booking.Id }, new ApiResponse<BookingDto>
                {
                    Success = true,
                    Message = "Booking created successfully",
                    Data = booking
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
                _logger.LogError(ex, "Failed to create booking");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while creating booking"
                });
            }
        }

        /// <summary>
        /// Update booking (Receptionist, Manager, Admin)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Receptionist,Manager,Admin")]
        public async Task<IActionResult> UpdateBooking(long id, [FromBody] UpdateBookingDto dto)
        {
            try
            {
                var booking = await _bookingService.UpdateBookingAsync(id, dto);
                return Ok(new ApiResponse<BookingDto>
                {
                    Success = true,
                    Message = "Booking updated successfully",
                    Data = booking
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
                _logger.LogError(ex, "Failed to update booking {BookingId}", id);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while updating booking"
                });
            }
        }

        /// <summary>
        /// Cancel booking
        /// </summary>
        [HttpPut("{id}/cancel")]
        [Authorize]
        public async Task<IActionResult> CancelBooking(long id)
        {
            try
            {
                var success = await _bookingService.CancelBookingAsync(id);
                if (success)
                {
                    return Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "Booking cancelled successfully"
                    });
                }
                else
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Failed to cancel booking"
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
                _logger.LogError(ex, "Failed to cancel booking {BookingId}", id);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while cancelling booking"
                });
            }
        }

        /// <summary>
        /// Check-in guest (Receptionist, Manager, Admin)
        /// </summary>
        [HttpPost("{id}/checkin")]
        [Authorize(Roles = "Receptionist,Manager,Admin")]
        public async Task<IActionResult> CheckIn(long id)
        {
            try
            {
                var booking = await _bookingService.CheckInAsync(id);
                return Ok(new ApiResponse<BookingDto>
                {
                    Success = true,
                    Message = "Check-in successful",
                    Data = booking
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
                _logger.LogError(ex, "Failed to check-in booking {BookingId}", id);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred during check-in"
                });
            }
        }

        /// <summary>
        /// Check-out guest - Auto-creates housekeeping task (Receptionist, Manager, Admin)
        /// </summary>
        [HttpPost("{id}/checkout")]
        [Authorize(Roles = "Receptionist,Manager,Admin")]
        public async Task<IActionResult> CheckOut(long id)
        {
            try
            {
                var booking = await _bookingService.CheckOutAsync(id);
                return Ok(new ApiResponse<BookingDto>
                {
                    Success = true,
                    Message = "Check-out successful. Housekeeping tasks created automatically.",
                    Data = booking
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
                _logger.LogError(ex, "Failed to check-out booking {BookingId}", id);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred during check-out"
                });
            }
        }

        /// <summary>
        /// Get booking statistics (Manager, Admin)
        /// </summary>
        [HttpGet("statistics")]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> GetBookingStatistics()
        {
            try
            {
                // Placeholder for statistics - to be implemented
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Statistics endpoint - to be implemented",
                    Data = new { Message = "Feature coming soon" }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get booking statistics");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving statistics"
                });
            }
        }
    }
}
