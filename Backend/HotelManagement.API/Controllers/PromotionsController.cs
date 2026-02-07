using HotelManagement.API.Exceptions;
using HotelManagement.API.Models.DTOs.Common;
using HotelManagement.API.Models.DTOs.Promotion;
using HotelManagement.API.Models.Entities;
using HotelManagement.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PromotionsController : ControllerBase
    {
        private readonly IPromotionService _promotionService;
        private readonly ILogger<PromotionsController> _logger;

        public PromotionsController(IPromotionService promotionService, ILogger<PromotionsController> logger)
        {
            _promotionService = promotionService;
            _logger = logger;
        }

        /// <summary>
        /// Get all promotions (Public - anyone can view)
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllPromotions()
        {
            try
            {
                var promotions = await _promotionService.GetAllPromotionsAsync();
                return Ok(new ApiResponse<IEnumerable<Promotion>> { Success = true, Data = promotions });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get all promotions");
                return StatusCode(500, new ApiResponse<object> { Success = false, Message = "An error occurred" });
            }
        }

        /// <summary>
        /// Get active promotions only (Public)
        /// </summary>
        [HttpGet("active")]
        [AllowAnonymous]
        public async Task<IActionResult> GetActivePromotions()
        {
            try
            {
                var promotions = await _promotionService.GetActivePromotionsAsync();
                return Ok(new ApiResponse<IEnumerable<Promotion>> { Success = true, Data = promotions });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get active promotions");
                return StatusCode(500, new ApiResponse<object> { Success = false, Message = "An error occurred" });
            }
        }

        /// <summary>
        /// Get promotion by ID (Public)
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPromotionById(long id)
        {
            try
            {
                var promotion = await _promotionService.GetPromotionByIdAsync(id);
                if (promotion == null)
                {
                    return NotFound(new ApiResponse<object> { Success = false, Message = $"Promotion with ID {id} not found" });
                }

                return Ok(new ApiResponse<Promotion> { Success = true, Data = promotion });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get promotion {PromotionId}", id);
                return StatusCode(500, new ApiResponse<object> { Success = false, Message = "An error occurred" });
            }
        }

        /// <summary>
        /// Get promotion by code (Public)
        /// </summary>
        [HttpGet("code/{code}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPromotionByCode(string code)
        {
            try
            {
                var promotion = await _promotionService.GetPromotionByCodeAsync(code);
                if (promotion == null)
                {
                    return NotFound(new ApiResponse<object> { Success = false, Message = $"Promotion with code '{code}' not found" });
                }

                return Ok(new ApiResponse<Promotion> { Success = true, Data = promotion });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get promotion by code {Code}", code);
                return StatusCode(500, new ApiResponse<object> { Success = false, Message = "An error occurred" });
            }
        }

        /// <summary>
        /// Validate promotion code (Authenticated users)
        /// </summary>
        [HttpPost("validate")]
        [Authorize]
        public async Task<IActionResult> ValidatePromotion([FromBody] ValidatePromotionDto dto)
        {
            try
            {
                var isValid = await _promotionService.ValidatePromotionAsync(dto.Code, dto.BookingAmount);
                if (isValid)
                {
                    var discount = await _promotionService.CalculateDiscountAsync(dto.Code, dto.BookingAmount);
                    return Ok(new ApiResponse<object> 
                    { 
                        Success = true, 
                        Message = "Promotion is valid",
                        Data = new { IsValid = true, DiscountAmount = discount }
                    });
                }
                else
                {
                    return BadRequest(new ApiResponse<object> { Success = false, Message = "Promotion is invalid or expired" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to validate promotion {Code}", dto.Code);
                return StatusCode(500, new ApiResponse<object> { Success = false, Message = "An error occurred" });
            }
        }

        /// <summary>
        /// Calculate discount (Authenticated users)
        /// </summary>
        [HttpPost("calculate-discount")]
        [Authorize]
        public async Task<IActionResult> CalculateDiscount([FromBody] ValidatePromotionDto dto)
        {
            try
            {
                var discount = await _promotionService.CalculateDiscountAsync(dto.Code, dto.BookingAmount);
                return Ok(new ApiResponse<object> { Success = true, Data = new { DiscountAmount = discount } });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new ApiResponse<object> { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to calculate discount for {Code}", dto.Code);
                return StatusCode(500, new ApiResponse<object> { Success = false, Message = "An error occurred" });
            }
        }

        /// <summary>
        /// Create promotion (Manager/Admin only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> CreatePromotion([FromBody] CreatePromotionDto dto)
        {
            try
            {
                var promotion = new Promotion
                {
                    Code = dto.Code,
                    Type = dto.Type,
                    Value = dto.Value,
                    StartDate = dto.StartDate,
                    EndDate = dto.EndDate,
                    ConditionsJson = dto.ConditionsJson,
                    CreatedAt = DateTime.UtcNow
                };

                var created = await _promotionService.CreatePromotionAsync(promotion);
                return CreatedAtAction(nameof(GetPromotionById), new { id = created.Id }, new ApiResponse<Promotion> { Success = true, Message = "Promotion created successfully", Data = created });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new ApiResponse<object> { Success = false, Message = ex.Message, Errors = ex.Errors });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create promotion");
                return StatusCode(500, new ApiResponse<object> { Success = false, Message = "An error occurred" });
            }
        }

        /// <summary>
        /// Update promotion (Manager/Admin only)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> UpdatePromotion(long id, [FromBody] UpdatePromotionDto dto)
        {
            try
            {
                var existing = await _promotionService.GetPromotionByIdAsync(id);
                if (existing == null)
                {
                    return NotFound(new ApiResponse<object> { Success = false, Message = $"Promotion with ID {id} not found" });
                }

                if (dto.Code != null) existing.Code = dto.Code;
                if (dto.Type != null) existing.Type = dto.Type;
                if (dto.Value.HasValue) existing.Value = dto.Value.Value;
                if (dto.StartDate.HasValue) existing.StartDate = dto.StartDate.Value;
                if (dto.EndDate.HasValue) existing.EndDate = dto.EndDate.Value;
                if (dto.ConditionsJson != null) existing.ConditionsJson = dto.ConditionsJson;

                var updated = await _promotionService.UpdatePromotionAsync(id, existing);
                return Ok(new ApiResponse<Promotion> { Success = true, Message = "Promotion updated successfully", Data = updated });
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
                _logger.LogError(ex, "Failed to update promotion {PromotionId}", id);
                return StatusCode(500, new ApiResponse<object> { Success = false, Message = "An error occurred" });
            }
        }

        /// <summary>
        /// Delete promotion (Manager/Admin only)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> DeletePromotion(long id)
        {
            try
            {
                var success = await _promotionService.DeletePromotionAsync(id);
                if (success)
                {
                    return Ok(new ApiResponse<object> { Success = true, Message = "Promotion deleted successfully" });
                }
                else
                {
                    return BadRequest(new ApiResponse<object> { Success = false, Message = "Failed to delete promotion" });
                }
            }
            catch (NotFoundException ex)
            {
                return NotFound(new ApiResponse<object> { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete promotion {PromotionId}", id);
                return StatusCode(500, new ApiResponse<object> { Success = false, Message = "An error occurred" });
            }
        }
    }
}
