using HotelManagement.API.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly HotelDbContext _context;

        public TestController(HotelDbContext context)
        {
            _context = context;
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
    }
}
