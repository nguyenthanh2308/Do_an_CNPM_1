using HotelManagement.API.Exceptions;
using HotelManagement.API.Models.DTOs.Common;
using HotelManagement.API.Models.DTOs.RatePlan;
using HotelManagement.API.Models.Entities;
using HotelManagement.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RatePlansController : ControllerBase
    {
        private readonly IRatePlanService _ratePlanService;
        private readonly ILogger<RatePlansController> _logger;

        public RatePlansController(IRatePlanService ratePlanService, ILogger<RatePlansController> logger)
        {
            _ratePlanService = ratePlanService;
            _logger = logger;
        }

        /// <summary>
        /// Get all rate plans (Public)
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllRatePlans()
        {
            try
            {
                var ratePlans = await _ratePlanService.GetAllRatePlansAsync();
                return Ok(new ApiResponse<IEnumerable<RatePlan>> { Success = true, Data = ratePlans });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get all rate plans");
                return StatusCode(500, new ApiResponse<object> { Success = false, Message = "An error occurred" });
            }
        }

        /// <summary>
        /// Get rate plan by ID (Public)
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetRatePlanById(long id)
        {
            try
            {
                var ratePlan = await _ratePlanService.GetRatePlanByIdAsync(id);
                if (ratePlan == null)
                {
                    return NotFound(new ApiResponse<object> { Success = false, Message = $"Rate plan with ID {id} not found" });
                }

                return Ok(new ApiResponse<RatePlan> { Success = true, Data = ratePlan });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get rate plan {RatePlanId}", id);
                return StatusCode(500, new ApiResponse<object> { Success = false, Message = "An error occurred" });
            }
        }

        /// <summary>
        /// Get rate plans by room type (Public)
        /// </summary>
        [HttpGet("room-type/{roomTypeId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetRatePlansByRoomType(long roomTypeId)
        {
            try
            {
                var ratePlans = await _ratePlanService.GetRatePlansByRoomTypeAsync(roomTypeId);
                return Ok(new ApiResponse<IEnumerable<RatePlan>> { Success = true, Data = ratePlans });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get rate plans for room type {RoomTypeId}", roomTypeId);
                return StatusCode(500, new ApiResponse<object> { Success = false, Message = "An error occurred" });
            }
        }

        /// <summary>
        /// Get active rate plans for a specific date (Public)
        /// </summary>
        [HttpGet("active")]
        [AllowAnonymous]
        public async Task<IActionResult> GetActiveRatePlans([FromQuery] DateTime date)
        {
            try
            {
                var ratePlans = await _ratePlanService.GetActiveRatePlansAsync(date);
                return Ok(new ApiResponse<IEnumerable<RatePlan>> { Success = true, Data = ratePlans });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get active rate plans for date {Date}", date);
                return StatusCode(500, new ApiResponse<object> { Success = false, Message = "An error occurred" });
            }
        }

        /// <summary>
        /// Get best rate plan for booking (Public)
        /// </summary>
        [HttpGet("best-rate")]
        [AllowAnonymous]
        public async Task<IActionResult> GetBestRatePlan([FromQuery] long roomTypeId, [FromQuery] DateTime checkIn, [FromQuery] DateTime checkOut)
        {
            try
            {
                var ratePlan = await _ratePlanService.GetBestRatePlanAsync(roomTypeId, checkIn, checkOut);
                if (ratePlan == null)
                {
                    return NotFound(new ApiResponse<object> { Success = false, Message = "No suitable rate plan found for the specified dates" });
                }

                return Ok(new ApiResponse<RatePlan> { Success = true, Data = ratePlan });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get best rate plan for room type {RoomTypeId}", roomTypeId);
                return StatusCode(500, new ApiResponse<object> { Success = false, Message = "An error occurred" });
            }
        }

        /// <summary>
        /// Calculate rate for a booking period (Authenticated)
        /// </summary>
        [HttpPost("calculate-rate")]
        [Authorize]
        public async Task<IActionResult> CalculateRate([FromQuery] long ratePlanId, [FromQuery] DateTime checkIn, [FromQuery] DateTime checkOut)
        {
            try
            {
                var totalRate = await _ratePlanService.CalculateRateAsync(ratePlanId, checkIn, checkOut);
                return Ok(new ApiResponse<object> 
                { 
                    Success = true, 
                    Data = new 
                    { 
                        RatePlanId = ratePlanId,
                        CheckIn = checkIn,
                        CheckOut = checkOut,
                        TotalRate = totalRate,
                        Nights = (checkOut - checkIn).Days
                    }
                });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new ApiResponse<object> { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to calculate rate for rate plan {RatePlanId}", ratePlanId);
                return StatusCode(500, new ApiResponse<object> { Success = false, Message = "An error occurred" });
            }
        }

        /// <summary>
        /// Create rate plan (Manager/Admin only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> CreateRatePlan([FromBody] CreateRatePlanDto dto)
        {
            try
            {
                var ratePlan = new RatePlan
                {
                    RoomTypeId = dto.RoomTypeId,
                    Name = dto.Name,
                    Type = dto.Type,
                    FreeCancelUntilHours = dto.FreeCancelUntilHours,
                    StartDate = dto.StartDate,
                    EndDate = dto.EndDate,
                    Price = dto.Price,
                    WeekendRuleJson = dto.WeekendRuleJson,
                    CreatedAt = DateTime.UtcNow
                };

                var created = await _ratePlanService.CreateRatePlanAsync(ratePlan);
                return CreatedAtAction(nameof(GetRatePlanById), new { id = created.Id }, new ApiResponse<RatePlan> { Success = true, Message = "Rate plan created successfully", Data = created });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new ApiResponse<object> { Success = false, Message = ex.Message, Errors = ex.Errors });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new ApiResponse<object> { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create rate plan");
                return StatusCode(500, new ApiResponse<object> { Success = false, Message = "An error occurred" });
            }
        }

        /// <summary>
        /// Update rate plan (Manager/Admin only)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> UpdateRatePlan(long id, [FromBody] UpdateRatePlanDto dto)
        {
            try
            {
                var existing = await _ratePlanService.GetRatePlanByIdAsync(id);
                if (existing == null)
                {
                    return NotFound(new ApiResponse<object> { Success = false, Message = $"Rate plan with ID {id} not found" });
                }

                if (dto.Name != null) existing.Name = dto.Name;
                if (dto.Type != null) existing.Type = dto.Type;
                if (dto.FreeCancelUntilHours.HasValue) existing.FreeCancelUntilHours = dto.FreeCancelUntilHours;
                if (dto.StartDate.HasValue) existing.StartDate = dto.StartDate.Value;
                if (dto.EndDate.HasValue) existing.EndDate = dto.EndDate.Value;
                if (dto.Price.HasValue) existing.Price = dto.Price.Value;
                if (dto.WeekendRuleJson != null) existing.WeekendRuleJson = dto.WeekendRuleJson;

                var updated = await _ratePlanService.UpdateRatePlanAsync(id, existing);
                return Ok(new ApiResponse<RatePlan> { Success = true, Message = "Rate plan updated successfully", Data = updated });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new ApiResponse<object> { Success = false, Message = ex.Message });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new ApiResponse<object> { Success = false, Message = ex.Message, Errors = ex.Errors });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update rate plan {RatePlanId}", id);
                return StatusCode(500, new ApiResponse<object> { Success = false, Message = "An error occurred" });
            }
        }

        /// <summary>
        /// Delete rate plan (Manager/Admin only)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> DeleteRatePlan(long id)
        {
            try
            {
                var success = await _ratePlanService.DeleteRatePlanAsync(id);
                if (success)
                {
                    return Ok(new ApiResponse<object> { Success = true, Message = "Rate plan deleted successfully" });
                }
                else
                {
                    return BadRequest(new ApiResponse<object> { Success = false, Message = "Failed to delete rate plan" });
                }
            }
            catch (NotFoundException ex)
            {
                return NotFound(new ApiResponse<object> { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete rate plan {RatePlanId}", id);
                return StatusCode(500, new ApiResponse<object> { Success = false, Message = "An error occurred" });
            }
        }
    }
}
