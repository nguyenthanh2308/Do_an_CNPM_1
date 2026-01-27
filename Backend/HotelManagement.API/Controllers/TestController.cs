using HotelManagement.API.Data;
using HotelManagement.API.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly HotelDbContext _context;
        private readonly IUnitOfWork _unitOfWork;

        public TestController(HotelDbContext context, IUnitOfWork unitOfWork)
        {
            _context = context;
            _unitOfWork = unitOfWork;
        }

        [HttpGet("db-check")]
        public async Task<IActionResult> CheckDatabase()
        {
            try
            {
                // Try to connect and count users
                var userCount = await _context.Users.CountAsync();
                return Ok(new { status = "Success", message = $"Connected to Database 'hotel_management_db'. Users count: {userCount}" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "Error", message = ex.Message });
            }
        }

        [HttpGet("repository-check")]
        public async Task<IActionResult> CheckRepositories()
        {
            try
            {
                // Test Unit of Work and Repositories
                var userCount = await _unitOfWork.Users.CountAsync(null);
                var hotelCount = await _unitOfWork.Hotels.CountAsync(null);
                var roomCount = await _unitOfWork.Rooms.CountAsync(null);
                var bookingCount = await _unitOfWork.Bookings.CountAsync(null);

                return Ok(new
                {
                    status = "Success",
                    message = "Repository Pattern working correctly",
                    counts = new
                    {
                        users = userCount,
                        hotels = hotelCount,
                        rooms = roomCount,
                        bookings = bookingCount
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "Error", message = ex.Message, stackTrace = ex.StackTrace });
            }
        }
    }
}
