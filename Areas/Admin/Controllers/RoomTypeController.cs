using System;
using System.Linq;
using System.Threading.Tasks;
using HotelManagementSystem.Models.ViewModels.RoomType;
using HotelManagementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HotelManagementSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/[controller]/[action]")]
    [Authorize(Roles = "Admin,Manager")]
    public class RoomTypeController : Controller
    {
        private readonly IRoomTypeService _roomTypeService;
        private readonly IHotelService _hotelService;
        private readonly IAmenityService _amenityService;

        public RoomTypeController(
            IRoomTypeService roomTypeService,
            IHotelService hotelService,
            IAmenityService amenityService)
        {
            _roomTypeService = roomTypeService;
            _hotelService = hotelService;
            _amenityService = amenityService;
        }

        // GET: Admin/RoomType/Index
        [HttpGet]
        public async Task<IActionResult> Index(long? hotelId)
        {
            var roomTypes = await _roomTypeService.GetAllAsync(hotelId);
            
            // Load hotels for filter
            var hotels = await _hotelService.GetAllAsync();
            ViewBag.Hotels = new SelectList(hotels, "Id", "Name", hotelId);
            ViewBag.SelectedHotelId = hotelId;

            return View(roomTypes);
        }

        // GET: Admin/RoomType/Details/5
        [HttpGet("{id:long}")]
        public async Task<IActionResult> Details(long id)
        {
            var roomType = await _roomTypeService.GetByIdAsync(id);
            if (roomType == null)
            {
                return NotFound();
            }

            return View(roomType);
        }

        // GET: Admin/RoomType/Create
        [HttpGet]
        public async Task<IActionResult> Create(long? hotelId)
        {
            await LoadHotelsAndAmenitiesAsync();

            var model = new RoomTypeViewModel
            {
                HotelId = hotelId ?? 0,
                Capacity = 2,
                BasePrice = 0
            };

            return View(model);
        }

        // POST: Admin/RoomType/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RoomTypeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadHotelsAndAmenitiesAsync();
                return View(model);
            }

            try
            {
                await _roomTypeService.CreateAsync(model);
                TempData["SuccessMessage"] = $"Loại phòng '{model.Name}' đã được tạo thành công!";
                return RedirectToAction(nameof(Index), new { hotelId = model.HotelId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                await LoadHotelsAndAmenitiesAsync();
                return View(model);
            }
        }

        // GET: Admin/RoomType/Edit/5
        [HttpGet("{id:long}")]
        public async Task<IActionResult> Edit(long id)
        {
            var roomType = await _roomTypeService.GetByIdAsync(id);
            if (roomType == null)
            {
                return NotFound();
            }

            await LoadHotelsAndAmenitiesAsync();
            return View(roomType);
        }

        // POST: Admin/RoomType/Edit/5
        [HttpPost("{id:long}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, RoomTypeViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                await LoadHotelsAndAmenitiesAsync();
                return View(model);
            }

            try
            {
                var updated = await _roomTypeService.UpdateAsync(model);
                if (updated == null)
                {
                    return NotFound();
                }

                TempData["SuccessMessage"] = $"Loại phòng '{model.Name}' đã được cập nhật thành công!";
                return RedirectToAction(nameof(Index), new { hotelId = model.HotelId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                await LoadHotelsAndAmenitiesAsync();
                return View(model);
            }
        }

        // GET: Admin/RoomType/Delete/5
        [HttpGet("{id:long}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(long id)
        {
            var roomType = await _roomTypeService.GetByIdAsync(id);
            if (roomType == null)
            {
                return NotFound();
            }

            return View(roomType);
        }

        // POST: Admin/RoomType/Delete/5
        [HttpPost("{id:long}")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var roomType = await _roomTypeService.GetByIdAsync(id);
            if (roomType == null)
            {
                return NotFound();
            }

            try
            {
                var success = await _roomTypeService.DeleteAsync(id);
                if (!success)
                {
                    return NotFound();
                }

                TempData["SuccessMessage"] = "Loại phòng đã được xóa thành công!";
                return RedirectToAction(nameof(Index), new { hotelId = roomType.HotelId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction(nameof(Delete), new { id });
            }
        }

        private async Task LoadHotelsAndAmenitiesAsync()
        {
            var hotels = await _hotelService.GetAllAsync();
            var amenities = await _amenityService.GetAllAsync();

            ViewBag.Hotels = new SelectList(hotels, "Id", "Name");
            ViewBag.AllAmenities = amenities.Select(a => new AmenityCheckboxViewModel
            {
                Id = a.Id,
                Name = a.Name,
                IsSelected = false
            }).ToList();
        }
    }
}
