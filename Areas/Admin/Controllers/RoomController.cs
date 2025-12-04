using System.Threading.Tasks;
using HotelManagementSystem.Models.Entities;
using HotelManagementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HotelManagementSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/[controller]/[action]")]
    [Authorize(Roles = "Admin,Manager")] // Chỉ Admin và Manager được truy cập
    public class RoomController : Controller
    {
        private readonly IRoomService _roomService;
        private readonly IHotelService _hotelService;
        private readonly IRoomTypeService _roomTypeService;

        public RoomController(
            IRoomService roomService,
            IHotelService hotelService,
            IRoomTypeService roomTypeService)
        {
            _roomService = roomService;
            _hotelService = hotelService;
            _roomTypeService = roomTypeService;
        }

        // GET: Admin/Room/Index
        [HttpGet]
        public async Task<IActionResult> Index(long? hotelId, long? roomTypeId)
        {
            var rooms = await _roomService.GetAllAsync(hotelId, roomTypeId);
            return View(rooms);   // View mạnh kiểu List<Room>
        }

        // GET: Admin/Room/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadDropdownsAsync();
            
            var model = new Room
            {
                Status = "Vacant" // mặc định
            };
            return View(model);
        }

        // POST: Admin/Room/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("HotelId,RoomTypeId,Number,Floor,Status")] Room model)
        {
            ModelState.Remove("Hotel");
            ModelState.Remove("RoomType");

            if (!ModelState.IsValid)
            {
                await LoadDropdownsAsync();
                return View(model);
            }

            await _roomService.CreateAsync(model);
            TempData["SuccessMessage"] = $"Đã tạo phòng {model.Number} thành công!";
            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/Room/Edit/5
        [HttpGet("{id:long}")]
        public async Task<IActionResult> Edit(long id)
        {
            var room = await _roomService.GetByIdAsync(id);
            if (room == null)
            {
                return NotFound();
            }

            await LoadDropdownsAsync(room.HotelId, room.RoomTypeId);
            return View(room);
        }

        // POST: Admin/Room/Edit/5
        [HttpPost("{id:long}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id,
            [Bind("Id,HotelId,RoomTypeId,Number,Floor,Status")] Room model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            ModelState.Remove("Hotel");
            ModelState.Remove("RoomType");

            if (!ModelState.IsValid)
            {
                await LoadDropdownsAsync(model.HotelId, model.RoomTypeId);
                return View(model);
            }

            var updated = await _roomService.UpdateAsync(model);
            if (updated == null)
            {
                return NotFound();
            }

            TempData["SuccessMessage"] = $"Đã cập nhật phòng {model.Number} thành công!";
            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/Room/Delete/5
        [HttpPost("{id:long}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(long id)
        {
            var success = await _roomService.DeleteAsync(id);
            if (!success)
            {
                return NotFound();
            }

            TempData["SuccessMessage"] = "Đã xóa phòng thành công!";
            return RedirectToAction(nameof(Index));
        }

        // Helper method to load dropdowns
        private async Task LoadDropdownsAsync(long? selectedHotelId = null, long? selectedRoomTypeId = null)
        {
            var hotels = await _hotelService.GetAllAsync();
            var roomTypes = await _roomTypeService.GetAllAsync();

            ViewBag.Hotels = new SelectList(hotels, "Id", "Name", selectedHotelId);
            ViewBag.RoomTypes = new SelectList(roomTypes, "Id", "Name", selectedRoomTypeId);
            
            ViewBag.StatusList = new SelectList(new[]
            {
                new { Value = "Vacant", Text = "Trống" },
                new { Value = "Occupied", Text = "Đang Ở" },
                new { Value = "Maintenance", Text = "Bảo Trì" },
                new { Value = "OutOfOrder", Text = "Hỏng" }
            }, "Value", "Text");
        }
    }
}
