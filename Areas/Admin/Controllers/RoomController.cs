using System;
using System.IO;
using System.Threading.Tasks;
using HotelManagementSystem.Models.Entities;
using HotelManagementSystem.Models.ViewModels.RoomVM;
using HotelManagementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HotelManagementSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/[controller]/[action]")]
    [Authorize(Roles = "Admin,Manager")]
    public class RoomController : Controller
    {
        private readonly IRoomService _roomService;
        private readonly IHotelService _hotelService;
        private readonly IRoomTypeService _roomTypeService;
        private readonly IWebHostEnvironment _environment;

        public RoomController(
            IRoomService roomService,
            IHotelService hotelService,
            IRoomTypeService roomTypeService,
            IWebHostEnvironment environment)
        {
            _roomService = roomService;
            _hotelService = hotelService;
            _roomTypeService = roomTypeService;
            _environment = environment;
        }

        // GET: Admin/Room/Index
        [HttpGet]
        public async Task<IActionResult> Index(long? hotelId, long? roomTypeId)
        {
            var rooms = await _roomService.GetAllAsync(hotelId, roomTypeId);
            return View(rooms);
        }

        // GET: Admin/Room/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadDropdownsAsync();
            
            var model = new RoomViewModel
            {
                Status = "Vacant"
            };
            return View(model);
        }

        // POST: Admin/Room/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RoomViewModel model)
        {
            ModelState.Remove("ImageFile");
            ModelState.Remove("HotelName");
            ModelState.Remove("RoomTypeName");

            if (!ModelState.IsValid)
            {
                await LoadDropdownsAsync();
                return View(model);
            }

            // Handle image upload
            string? imageUrl = null;
            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                imageUrl = await SaveImageAsync(model.ImageFile);
            }

            var room = new Room
            {
                HotelId = model.HotelId,
                RoomTypeId = model.RoomTypeId,
                Number = model.Number,
                Floor = model.Floor,
                Status = model.Status,
                ImageUrl = imageUrl,
                CreatedAt = DateTime.Now
            };

            await _roomService.CreateAsync(room);
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

            var model = new RoomViewModel
            {
                Id = room.Id,
                HotelId = room.HotelId,
                RoomTypeId = room.RoomTypeId,
                Number = room.Number,
                Floor = room.Floor,
                Status = room.Status,
                ImageUrl = room.ImageUrl,
                CreatedAt = room.CreatedAt
            };

            await LoadDropdownsAsync(room.HotelId, room.RoomTypeId);
            return View(model);
        }

        // POST: Admin/Room/Edit/5
        [HttpPost("{id:long}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, RoomViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            ModelState.Remove("ImageFile");
            ModelState.Remove("HotelName");
            ModelState.Remove("RoomTypeName");

            if (!ModelState.IsValid)
            {
                await LoadDropdownsAsync(model.HotelId, model.RoomTypeId);
                return View(model);
            }

            // Handle image upload
            string? imageUrl = model.ImageUrl;
            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                imageUrl = await SaveImageAsync(model.ImageFile);
            }

            var room = new Room
            {
                Id = model.Id,
                HotelId = model.HotelId,
                RoomTypeId = model.RoomTypeId,
                Number = model.Number,
                Floor = model.Floor,
                Status = model.Status,
                ImageUrl = imageUrl
            };

            var updated = await _roomService.UpdateAsync(room);
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

        // Helper method to save image
        private async Task<string> SaveImageAsync(Microsoft.AspNetCore.Http.IFormFile imageFile)
        {
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "rooms");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(imageFile.FileName)}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }

            return $"/uploads/rooms/{uniqueFileName}";
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
