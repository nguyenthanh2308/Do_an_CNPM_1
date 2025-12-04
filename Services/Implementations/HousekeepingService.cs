using HotelManagementSystem.Data;
using HotelManagementSystem.Models.Entities;
using HotelManagementSystem.Models.ViewModels.Housekeeping;
using HotelManagementSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelManagementSystem.Services.Implementations;

public class HousekeepingService : IHousekeepingService
{
    private readonly HotelDbContext _context;

    public HousekeepingService(HotelDbContext context)
    {
        _context = context;
    }

    public async Task<List<HousekeepingTaskViewModel>> GetAllAsync(
        string? status = null,
        string? taskType = null,
        string? priority = null,
        ulong? roomId = null,
        ulong? assignedUserId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null)
    {
        var query = _context.HousekeepingTasks
            .Include(t => t.Room)
                .ThenInclude(r => r.RoomType)
            .Include(t => t.Room.Hotel)
            .Include(t => t.AssignedToUser)
            .Include(t => t.Booking)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(t => t.Status == status);
        }

        if (!string.IsNullOrEmpty(taskType))
        {
            query = query.Where(t => t.TaskType == taskType);
        }

        if (!string.IsNullOrEmpty(priority))
        {
            query = query.Where(t => t.Priority == priority);
        }

        if (roomId.HasValue)
        {
            query = query.Where(t => t.RoomId == (long)roomId.Value);
        }

        if (assignedUserId.HasValue)
        {
            query = query.Where(t => t.AssignedToUserId == (long)assignedUserId.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(t => t.ScheduledAt >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(t => t.ScheduledAt <= toDate.Value);
        }

        var tasks = await query
            .OrderByDescending(t => t.Priority == "Urgent")
            .ThenByDescending(t => t.Priority == "High")
            .ThenBy(t => t.ScheduledAt)
            .ToListAsync();

        return tasks.Select(MapToViewModel).ToList();
    }

    public async Task<HousekeepingTaskViewModel?> GetByIdAsync(ulong id)
    {
        var task = await _context.HousekeepingTasks
            .Include(t => t.Room)
                .ThenInclude(r => r.RoomType)
            .Include(t => t.Room.Hotel)
            .Include(t => t.AssignedToUser)
            .Include(t => t.Booking)
            .FirstOrDefaultAsync(t => t.Id == (long)id);

        return task == null ? null : MapToViewModel(task);
    }

    public async Task<List<HousekeepingTaskViewModel>> GetMyTasksAsync(ulong userId, string? status = null)
    {
        var query = _context.HousekeepingTasks
            .Include(t => t.Room)
                .ThenInclude(r => r.RoomType)
            .Include(t => t.Room.Hotel)
            .Include(t => t.AssignedToUser)
            .Where(t => t.AssignedToUserId == (long)userId);

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(t => t.Status == status);
        }

        var tasks = await query
            .OrderByDescending(t => t.Priority == "Urgent")
            .ThenByDescending(t => t.Priority == "High")
            .ThenBy(t => t.ScheduledAt)
            .ToListAsync();

        return tasks.Select(MapToViewModel).ToList();
    }

    public async Task<HousekeepingTaskViewModel?> GetPendingTaskByRoomAsync(ulong roomId)
    {
        var task = await _context.HousekeepingTasks
            .Include(t => t.Room)
                .ThenInclude(r => r.RoomType)
            .Include(t => t.Room.Hotel)
            .Include(t => t.AssignedToUser)
            .Where(t => t.RoomId == (long)roomId && t.Status == "Pending")
            .OrderBy(t => t.ScheduledAt)
            .FirstOrDefaultAsync();

        return task == null ? null : MapToViewModel(task);
    }

    public async Task<HousekeepingTaskViewModel?> CreateTaskAsync(
        ulong roomId,
        string taskType,
        string priority,
        DateTime scheduledAt,
        ulong? assignedUserId = null,
        string? notes = null,
        ulong? bookingId = null)
    {
        // Validate room exists
        var room = await _context.Rooms.FindAsync((long)roomId);
        if (room == null)
        {
            return null;
        }

        // Validate assigned user if provided
        if (assignedUserId.HasValue)
        {
            var user = await _context.Users.FindAsync((long)assignedUserId.Value);
            if (user == null || user.Role != "Housekeeping")
            {
                return null; // User không tồn tại hoặc không phải Housekeeping role
            }
        }

        var task = new HousekeepingTask
        {
            RoomId = (long)roomId,
            TaskType = taskType,
            Priority = priority,
            ScheduledAt = scheduledAt,
            AssignedToUserId = assignedUserId.HasValue ? (long)assignedUserId.Value : null,
            Notes = notes,
            BookingId = bookingId.HasValue ? (long)bookingId.Value : null,
            Status = "Pending",
            CreatedAt = DateTime.Now
        };

        _context.HousekeepingTasks.Add(task);
        await _context.SaveChangesAsync();

        return await GetByIdAsync((ulong)task.Id);
    }

    public async Task<HousekeepingTaskViewModel?> AutoCreateCheckoutTaskAsync(ulong bookingId, ulong roomId)
    {
        // Kiểm tra đã có task CheckOut cho booking này chưa
        var existingTask = await _context.HousekeepingTasks
            .FirstOrDefaultAsync(t => t.BookingId == (long)bookingId && t.TaskType == "CheckOut");

        if (existingTask != null)
        {
            return MapToViewModel(existingTask);
        }

        // Tạo task CheckOut với priority High, scheduled ngay
        return await CreateTaskAsync(
            roomId: roomId,
            taskType: "CheckOut",
            priority: "High",
            scheduledAt: DateTime.Now,
            assignedUserId: null, // Chưa assign, Manager sẽ assign sau
            notes: "Tự động tạo sau khi khách checkout",
            bookingId: bookingId
        );
    }

    public async Task<bool> AssignTaskAsync(ulong taskId, ulong userId)
    {
        var task = await _context.HousekeepingTasks.FindAsync((long)taskId);
        if (task == null || task.Status != "Pending")
        {
            return false;
        }

        // Validate user is Housekeeping
        var user = await _context.Users.FindAsync((long)userId);
        if (user == null || user.Role != "Housekeeping")
        {
            return false;
        }

        task.AssignedToUserId = (long)userId;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateStatusAsync(ulong taskId, string newStatus, string? notes = null)
    {
        var task = await _context.HousekeepingTasks.FindAsync((long)taskId);
        if (task == null)
        {
            return false;
        }

        // Validate status transition
        var validStatuses = new[] { "Pending", "InProgress", "Completed", "Cancelled" };
        if (!validStatuses.Contains(newStatus))
        {
            return false;
        }

        task.Status = newStatus;

        if (newStatus == "Completed")
        {
            task.CompletedAt = DateTime.Now;
        }

        if (!string.IsNullOrEmpty(notes))
        {
            task.Notes = string.IsNullOrEmpty(task.Notes) ? notes : task.Notes + "\n" + notes;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> StartTaskAsync(ulong taskId, ulong userId)
    {
        var task = await _context.HousekeepingTasks.FindAsync((long)taskId);
        if (task == null || task.Status != "Pending")
        {
            return false;
        }

        // Verify user is assigned
        if (task.AssignedToUserId != (long)userId)
        {
            return false;
        }

        task.Status = "InProgress";
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CompleteTaskAsync(ulong taskId, string? completionNotes = null)
    {
        var task = await _context.HousekeepingTasks.FindAsync((long)taskId);
        if (task == null || task.Status != "InProgress")
        {
            return false;
        }

        task.Status = "Completed";
        task.CompletedAt = DateTime.Now;

        if (!string.IsNullOrEmpty(completionNotes))
        {
            task.Notes = string.IsNullOrEmpty(task.Notes) 
                ? completionNotes 
                : task.Notes + "\n[Hoàn thành] " + completionNotes;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CancelTaskAsync(ulong taskId, string? reason = null)
    {
        var task = await _context.HousekeepingTasks.FindAsync((long)taskId);
        if (task == null || task.Status == "Completed")
        {
            return false;
        }

        task.Status = "Cancelled";

        if (!string.IsNullOrEmpty(reason))
        {
            task.Notes = string.IsNullOrEmpty(task.Notes) 
                ? $"[Hủy] {reason}" 
                : task.Notes + $"\n[Hủy] {reason}";
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<(int TotalTasks, int PendingTasks, int InProgressTasks, int CompletedTasks, int OverdueTasks)> GetStatisticsAsync(DateTime? date = null)
    {
        var targetDate = date ?? DateTime.Today;
        var startOfDay = targetDate.Date;
        var endOfDay = startOfDay.AddDays(1);

        // Don't include navigation properties for statistics - we only need counts
        var tasks = await _context.HousekeepingTasks
            .Where(t => t.ScheduledAt >= startOfDay && t.ScheduledAt < endOfDay)
            .Select(t => new { t.Status, t.ScheduledAt })
            .ToListAsync();

        var totalTasks = tasks.Count;
        var pendingTasks = tasks.Count(t => t.Status == "Pending");
        var inProgressTasks = tasks.Count(t => t.Status == "InProgress");
        var completedTasks = tasks.Count(t => t.Status == "Completed");
        var overdueTasks = tasks.Count(t => t.Status == "Pending" && t.ScheduledAt < DateTime.Now);

        return (totalTasks, pendingTasks, inProgressTasks, completedTasks, overdueTasks);
    }

    public async Task<List<(ulong RoomId, string RoomNumber, string Status, bool HasPendingTask)>> GetRoomStatusBoardAsync(ulong? hotelId = null)
    {
        var query = _context.Rooms.AsQueryable();

        if (hotelId.HasValue)
        {
            query = query.Where(r => r.HotelId == (long)hotelId.Value);
        }

        var rooms = await query.OrderBy(r => r.Number).ToListAsync();

        var result = new List<(ulong, string, string, bool)>();

        foreach (var room in rooms)
        {
            var hasPendingTask = await _context.HousekeepingTasks
                .AnyAsync(t => t.RoomId == room.Id && t.Status == "Pending");

            result.Add(((ulong)room.Id, room.Number, room.Status, hasPendingTask));
        }

        return result;
    }

    public async Task<(int CompletedTasks, double AvgCompletionMinutes)> GetUserPerformanceAsync(ulong userId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.HousekeepingTasks
            .Where(t => t.AssignedToUserId == (long)userId && t.Status == "Completed" && t.CompletedAt.HasValue);

        if (fromDate.HasValue)
        {
            query = query.Where(t => t.CompletedAt >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(t => t.CompletedAt <= toDate.Value);
        }

        var tasks = await query.ToListAsync();
        var completedTasks = tasks.Count;

        var avgMinutes = 0.0;
        if (completedTasks > 0)
        {
            var durations = tasks
                .Where(t => t.CompletedAt.HasValue)
                .Select(t => (t.CompletedAt!.Value - t.ScheduledAt).TotalMinutes)
                .ToList();

            avgMinutes = durations.Any() ? durations.Average() : 0;
        }

        return (completedTasks, avgMinutes);
    }

    // Helper method
    private HousekeepingTaskViewModel MapToViewModel(HousekeepingTask task)
    {
        var room = task.Room;
        var roomType = room.RoomType;
        var hotel = room.Hotel;
        var assignedUser = task.AssignedToUser;
        var booking = task.Booking;

        return new HousekeepingTaskViewModel
        {
            Id = (ulong)task.Id,
            RoomId = (ulong)task.RoomId,
            AssignedUserId = task.AssignedToUserId.HasValue ? (ulong)task.AssignedToUserId.Value : null,
            TaskType = task.TaskType,
            Status = task.Status,
            Priority = task.Priority,
            ScheduledAt = task.ScheduledAt,
            CompletedAt = task.CompletedAt,
            Notes = task.Notes,
            CreatedAt = task.CreatedAt,

            // Room info
            RoomNumber = room.Number,
            RoomTypeName = roomType?.Name ?? "N/A",
            HotelName = hotel?.Name ?? "N/A",
            RoomStatus = room.Status,

            // Assigned user info
            AssignedUserName = assignedUser?.Username,
            AssignedUserRole = assignedUser?.Role,

            // Booking info
            BookingId = task.BookingId.HasValue ? (ulong)task.BookingId.Value : null,
            BookingCode = booking != null ? $"BK-{booking.Id.ToString("D8")}" : null
        };
    }
}
