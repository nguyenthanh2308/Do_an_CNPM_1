using HotelManagement.API.Exceptions;
using HotelManagement.API.Models.DTOs.Common;
using HotelManagement.API.Models.DTOs.Payment;
using HotelManagement.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HotelManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvoicesController : ControllerBase
    {
        private readonly IInvoiceService _invoiceService;
        private readonly ILogger<InvoicesController> _logger;

        public InvoicesController(IInvoiceService invoiceService, ILogger<InvoicesController> logger)
        {
            _invoiceService = invoiceService;
            _logger = logger;
        }

        /// <summary>
        /// Generate invoice for a booking (Receptionist, Manager, Admin)
        /// </summary>
        [HttpPost("generate/{bookingId}")]
        [Authorize(Roles = "Receptionist,Manager,Admin")]
        public async Task<IActionResult> GenerateInvoice(long bookingId)
        {
            try
            {
                var invoice = await _invoiceService.GenerateInvoiceAsync(bookingId);
                return Ok(new ApiResponse<InvoiceDto>
                {
                    Success = true,
                    Message = "Invoice generated successfully",
                    Data = invoice
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
                _logger.LogError(ex, "Failed to generate invoice for booking {BookingId}", bookingId);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while generating invoice"
                });
            }
        }

        /// <summary>
        /// Get invoice by booking ID
        /// </summary>
        [HttpGet("booking/{bookingId}")]
        [Authorize(Roles = "Receptionist,Manager,Admin,Customer")]
        public async Task<IActionResult> GetInvoiceByBookingId(long bookingId)
        {
            try
            {
                var invoice = await _invoiceService.GetInvoiceByBookingIdAsync(bookingId);
                if (invoice == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Invoice for booking {bookingId} not found"
                    });
                }

                return Ok(new ApiResponse<InvoiceDto>
                {
                    Success = true,
                    Data = invoice
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get invoice for booking {BookingId}", bookingId);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving invoice"
                });
            }
        }

        /// <summary>
        /// Get invoice by ID
        /// </summary>
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetInvoiceById(long id)
        {
            try
            {
                var invoice = await _invoiceService.GetInvoiceByIdAsync(id);
                if (invoice == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Invoice with ID {id} not found"
                    });
                }

                // Check if customer owns this invoice (via booking)
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                if (userRole == "Customer")
                {
                    // Additional authorization check would be needed here
                    // to verify customer owns the booking associated with this invoice
                }

                return Ok(new ApiResponse<InvoiceDto>
                {
                    Success = true,
                    Data = invoice
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get invoice {InvoiceId}", id);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving invoice"
                });
            }
        }

        /// <summary>
        /// Get invoice by invoice number
        /// </summary>
        [HttpGet("number/{invoiceNumber}")]
        [Authorize(Roles = "Receptionist,Manager,Admin")]
        public async Task<IActionResult> GetInvoiceByNumber(string invoiceNumber)
        {
            try
            {
                var invoice = await _invoiceService.GetInvoiceByInvoiceNumberAsync(invoiceNumber);
                if (invoice == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Invoice with number {invoiceNumber} not found"
                    });
                }

                return Ok(new ApiResponse<InvoiceDto>
                {
                    Success = true,
                    Data = invoice
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get invoice by number {InvoiceNumber}", invoiceNumber);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving invoice"
                });
            }
        }

        /// <summary>
        /// Download invoice as PDF (Future implementation)
        /// </summary>
        [HttpGet("{id}/download")]
        [Authorize]
        public async Task<IActionResult> DownloadInvoice(long id)
        {
            try
            {
                // Placeholder for PDF generation
                await Task.CompletedTask;
                
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "PDF download feature - to be implemented",
                    Data = new { InvoiceId = id, Format = "PDF" }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to download invoice {InvoiceId}", id);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while downloading invoice"
                });
            }
        }

        /// <summary>
        /// Get my invoices (Customer)
        /// </summary>
        [HttpGet("my-invoices")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetMyInvoices()
        {
            try
            {
                // This would need to be implemented in the service
                // For now, return placeholder
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "My invoices endpoint - to be implemented",
                    Data = new List<object>()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get user invoices");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving invoices"
                });
            }
        }
    }
}
