using HotelManagement.API.Models.DTOs.Common;
using HotelManagement.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Manager,Admin")]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;
        private readonly ILogger<ReportsController> _logger;

        public ReportsController(IReportService reportService, ILogger<ReportsController> logger)
        {
            _reportService = reportService;
            _logger = logger;
        }

        /// <summary>
        /// Get revenue report for a date range
        /// </summary>
        [HttpGet("revenue")]
        public async Task<IActionResult> GetRevenueReport([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] long? hotelId = null)
        {
            try
            {
                if (startDate > endDate)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Start date must be before end date"
                    });
                }

                var report = await _reportService.GetRevenueByDateRangeAsync(startDate, endDate);
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = report
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get revenue report");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while generating revenue report"
                });
            }
        }

        /// <summary>
        /// Get occupancy report for a date range
        /// </summary>
        [HttpGet("occupancy")]
        public async Task<IActionResult> GetOccupancyReport([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] long? hotelId = null)
        {
            try
            {
                if (startDate > endDate)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Start date must be before end date"
                    });
                }

                var report = await _reportService.GetOccupancyReportAsync(startDate);
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = report
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get occupancy report");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while generating occupancy report"
                });
            }
        }

        /// <summary>
        /// Get popular room types report
        /// </summary>
        [HttpGet("popular-rooms")]
        public async Task<IActionResult> GetPopularRoomsReport([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] long? hotelId = null, [FromQuery] int top = 10)
        {
            try
            {
                if (startDate > endDate)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Start date must be before end date"
                    });
                }

                if (top <= 0)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Top parameter must be greater than 0"
                    });
                }

                var report = await _reportService.GetRoomPerformanceReportAsync(startDate, endDate);
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = report
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get popular rooms report");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while generating popular rooms report"
                });
            }
        }

        /// <summary>
        /// Get guest statistics report
        /// </summary>
        [HttpGet("guest-statistics")]
        public async Task<IActionResult> GetGuestStatistics([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                if (startDate > endDate)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Start date must be before end date"
                    });
                }

                var report = await _reportService.GetGuestStatisticsAsync();
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = report
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get guest statistics");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while generating guest statistics"
                });
            }
        }

        /// <summary>
        /// Get payment summary report
        /// </summary>
        [HttpGet("payment-summary")]
        public async Task<IActionResult> GetPaymentSummary([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] long? hotelId = null)
        {
            try
            {
                if (startDate > endDate)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Start date must be before end date"
                    });
                }

                var report = await _reportService.GetPaymentReportAsync(startDate, endDate);
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = report
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get payment summary");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while generating payment summary"
                });
            }
        }

        /// <summary>
        /// Get housekeeping performance report
        /// </summary>
        [HttpGet("housekeeping-performance")]
        public async Task<IActionResult> GetHousekeepingPerformance([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] long? userId = null)
        {
            try
            {
                if (startDate > endDate)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Start date must be before end date"
                    });
                }

                var report = await _reportService.GetHousekeepingPerformanceReportAsync(startDate, endDate);
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = report
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get housekeeping performance report");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while generating housekeeping performance report"
                });
            }
        }

        /// <summary>
        /// Export report to CSV
        /// </summary>
        [HttpGet("export")]
        public async Task<IActionResult> ExportReport([FromQuery] string reportType, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                if (startDate > endDate)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Start date must be before end date"
                    });
                }

                var pdfData = await _reportService.ExportReportToPdfAsync(reportType, startDate, endDate);
                
                return File(pdfData, "application/pdf", $"{reportType}_report_{DateTime.Now:yyyyMMdd}.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to export report");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while exporting report"
                });
            }
        }
    }
}
