using HotelManagementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HotelManagementSystem.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize] // Tất cả phải đăng nhập
public class HousekeepingController : Controller
{
    private readonly IHousekeepingService _housekeepingService;
    private readonly IRoomService _roomService;

    public HousekeepingController(IHousekeepingService housekeepingService, IRoomService roomService)
    {
        _housekeepingService = housekeepingService;
        _roomService = roomService;
    }

    // GET: /Admin/Housekeeping/Dashboard (Admin, Manager only)
    [Authorize(Policy = "ManagerOrAdmin")]
    public async Task<IActionResult> Dashboard(DateTime? date = null)
    {
        var targetDate = date ?? DateTime.Today;
        var stats = await _housekeepingService.GetStatisticsAsync(targetDate);

        ViewBag.TotalTasks = stats.TotalTasks;
        ViewBag.PendingTasks = stats.PendingTasks;
        ViewBag.InProgressTasks = stats.InProgressTasks;
        ViewBag.CompletedTasks = stats.CompletedTasks;
        ViewBag.OverdueTasks = stats.OverdueTasks;
        ViewBag.SelectedDate = targetDate;

        // Get room status board
        var roomStatus = await _housekeepingService.GetRoomStatusBoardAsync();
        ViewBag.RoomStatus = roomStatus;

        return View();
    }

    // GET: /Admin/Housekeeping (Admin, Manager - view all tasks)
    [Authorize(Policy = "ManagerOrAdmin")]
    public async Task<IActionResult> Index(
        string? status = null,
        string? taskType = null,
        string? priority = null,
        ulong? roomId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null)
    {
        var tasks = await _housekeepingService.GetAllAsync(status, taskType, priority, roomId, null, fromDate, toDate);

        ViewBag.CurrentStatus = status;
        ViewBag.CurrentTaskType = taskType;
        ViewBag.CurrentPriority = priority;
        ViewBag.FromDate = fromDate;
        ViewBag.ToDate = toDate;

        return View(tasks);
    }

    // GET: /Admin/Housekeeping/MyTasks (Housekeeping staff - view own tasks)
    [Authorize(Roles = "Housekeeping,Admin,Manager")]
    public async Task<IActionResult> MyTasks(string? status = null)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !ulong.TryParse(userIdClaim, out var userId))
        {
            return RedirectToAction("Login", "Auth", new { area = "" });
        }

        var tasks = await _housekeepingService.GetMyTasksAsync(userId, status);
        ViewBag.CurrentStatus = status;

        return View(tasks);
    }

    // GET: /Admin/Housekeeping/Details/5
    public async Task<IActionResult> Details(ulong id)
    {
        var task = await _housekeepingService.GetByIdAsync(id);
        if (task == null)
        {
            TempData["ErrorMessage"] = "Không tìm thấy task!";
            return RedirectToAction(nameof(Index));
        }

        // Housekeeping staff chỉ được xem task của mình
        if (User.IsInRole("Housekeeping") && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !ulong.TryParse(userIdClaim, out var userId) || task.AssignedUserId != userId)
            {
                TempData["ErrorMessage"] = "Bạn không có quyền xem task này!";
                return RedirectToAction(nameof(MyTasks));
            }
        }

        return View(task);
    }

    // GET: /Admin/Housekeeping/Create (Admin, Manager only)
    [Authorize(Policy = "ManagerOrAdmin")]
    public async Task<IActionResult> Create(ulong? roomId = null)
    {
        if (roomId.HasValue)
        {
            var room = await _roomService.GetByIdAsync((long)roomId.Value);
            ViewBag.SelectedRoom = room;
        }

        return View();
    }

    // POST: /Admin/Housekeeping/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "ManagerOrAdmin")]
    public async Task<IActionResult> Create(
        ulong roomId,
        string taskType,
        string priority,
        DateTime scheduledAt,
        ulong? assignedUserId = null,
        string? notes = null)
    {
        var task = await _housekeepingService.CreateTaskAsync(roomId, taskType, priority, scheduledAt, assignedUserId, notes);
        
        if (task == null)
        {
            TempData["ErrorMessage"] = "Không thể tạo task! Kiểm tra lại thông tin.";
            return RedirectToAction(nameof(Create), new { roomId });
        }

        TempData["SuccessMessage"] = $"Đã tạo task {task.TaskTypeDisplay} cho phòng {task.RoomNumber}!";
        return RedirectToAction(nameof(Details), new { id = task.Id });
    }

    // GET: /Admin/Housekeeping/Assign/5 (Admin, Manager only)
    [Authorize(Policy = "ManagerOrAdmin")]
    public async Task<IActionResult> Assign(ulong id)
    {
        var task = await _housekeepingService.GetByIdAsync(id);
        if (task == null)
        {
            TempData["ErrorMessage"] = "Không tìm thấy task!";
            return RedirectToAction(nameof(Index));
        }

        if (!task.CanReassign)
        {
            TempData["ErrorMessage"] = "Task này không thể phân công lại!";
            return RedirectToAction(nameof(Details), new { id });
        }

        return View(task);
    }

    // POST: /Admin/Housekeeping/Assign
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "ManagerOrAdmin")]
    public async Task<IActionResult> Assign(ulong id, ulong userId)
    {
        var success = await _housekeepingService.AssignTaskAsync(id, userId);
        
        if (!success)
        {
            TempData["ErrorMessage"] = "Không thể phân công task! User phải có role Housekeeping.";
        }
        else
        {
            TempData["SuccessMessage"] = "Đã phân công task thành công!";
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    // POST: /Admin/Housekeeping/Start/5 (Housekeeping staff)
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Housekeeping,Admin,Manager")]
    public async Task<IActionResult> Start(ulong id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !ulong.TryParse(userIdClaim, out var userId))
        {
            return RedirectToAction("Login", "Auth", new { area = "" });
        }

        var success = await _housekeepingService.StartTaskAsync(id, userId);
        
        if (!success)
        {
            TempData["ErrorMessage"] = "Không thể bắt đầu task! Task phải được giao cho bạn và ở trạng thái Pending.";
        }
        else
        {
            TempData["SuccessMessage"] = "Đã bắt đầu task!";
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    // POST: /Admin/Housekeeping/Complete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Housekeeping,Admin,Manager")]
    public async Task<IActionResult> Complete(ulong id, string? completionNotes = null)
    {
        var success = await _housekeepingService.CompleteTaskAsync(id, completionNotes);
        
        if (!success)
        {
            TempData["ErrorMessage"] = "Không thể hoàn thành task! Task phải ở trạng thái InProgress.";
        }
        else
        {
            TempData["SuccessMessage"] = "Đã hoàn thành task!";
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    // POST: /Admin/Housekeeping/Cancel/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "ManagerOrAdmin")]
    public async Task<IActionResult> Cancel(ulong id, string? reason = null)
    {
        var success = await _housekeepingService.CancelTaskAsync(id, reason);
        
        if (!success)
        {
            TempData["ErrorMessage"] = "Không thể hủy task!";
        }
        else
        {
            TempData["SuccessMessage"] = "Đã hủy task!";
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    // GET: /Admin/Housekeeping/RoomStatus (Admin, Manager only)
    [Authorize(Policy = "ManagerOrAdmin")]
    public async Task<IActionResult> RoomStatus(ulong? hotelId = null)
    {
        var roomStatus = await _housekeepingService.GetRoomStatusBoardAsync(hotelId);
        return View(roomStatus);
    }

    // API: Get user performance (Admin, Manager only)
    [HttpGet]
    [Authorize(Policy = "ManagerOrAdmin")]
    public async Task<IActionResult> GetUserPerformance(ulong userId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var performance = await _housekeepingService.GetUserPerformanceAsync(userId, fromDate, toDate);
        return Json(new
        {
            completedTasks = performance.CompletedTasks,
            avgCompletionMinutes = Math.Round(performance.AvgCompletionMinutes, 1)
        });
    }
}
