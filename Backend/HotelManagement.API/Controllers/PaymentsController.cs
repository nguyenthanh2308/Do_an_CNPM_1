using HotelManagement.API.Exceptions;
using HotelManagement.API.Models.DTOs.Common;
using HotelManagement.API.Models.DTOs.Payment;
using HotelManagement.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Receptionist,Manager,Admin")]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly ILogger<PaymentsController> _logger;

        public PaymentsController(IPaymentService paymentService, ILogger<PaymentsController> logger)
        {
            _paymentService = paymentService;
            _logger = logger;
        }

        /// <summary>
        /// Process payment for a booking
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ProcessPayment([FromBody] CreatePaymentDto dto)
        {
            try
            {
                var payment = await _paymentService.ProcessPaymentAsync(dto);
                return Ok(new ApiResponse<PaymentDto>
                {
                    Success = true,
                    Message = "Payment processed successfully",
                    Data = payment
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
                _logger.LogError(ex, "Failed to process payment");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while processing payment"
                });
            }
        }

        /// <summary>
        /// Get payments for a booking
        /// </summary>
        [HttpGet("booking/{bookingId}")]
        public async Task<IActionResult> GetPaymentsByBooking(long bookingId)
        {
            try
            {
                var payments = await _paymentService.GetPaymentsByBookingAsync(bookingId);
                return Ok(new ApiResponse<IEnumerable<PaymentDto>>
                {
                    Success = true,
                    Data = payments
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get payments for booking {BookingId}", bookingId);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving payments"
                });
            }
        }

        /// <summary>
        /// Get payment by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPaymentById(long id)
        {
            try
            {
                var payment = await _paymentService.GetPaymentByIdAsync(id);
                if (payment == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Payment with ID {id} not found"
                    });
                }

                return Ok(new ApiResponse<PaymentDto>
                {
                    Success = true,
                    Data = payment
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get payment {PaymentId}", id);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving payment"
                });
            }
        }

        /// <summary>
        /// Get total paid amount for a booking
        /// </summary>
        [HttpGet("booking/{bookingId}/total")]
        public async Task<IActionResult> GetTotalPaidForBooking(long bookingId)
        {
            try
            {
                var totalPaid = await _paymentService.GetTotalPaidForBookingAsync(bookingId);
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = new { BookingId = bookingId, TotalPaid = totalPaid }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get total paid for booking {BookingId}", bookingId);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving payment total"
                });
            }
        }

        /// <summary>
        /// Process refund for a payment
        /// </summary>
        [HttpPost("{id}/refund")]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> RefundPayment(long id, [FromBody] RefundPaymentDto dto)
        {
            try
            {
                var refund = await _paymentService.RefundPaymentAsync(id, dto.Reason);
                return Ok(new ApiResponse<PaymentDto>
                {
                    Success = true,
                    Message = "Refund processed successfully",
                    Data = refund
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
                _logger.LogError(ex, "Failed to process refund for payment {PaymentId}", id);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while processing refund"
                });
            }
        }
    }
}
