using HotelManagementSystem.Models.ViewModels.Housekeeping;

namespace HotelManagementSystem.Services.Interfaces;

public interface IHousekeepingService
{
    /// <summary>
    /// Lấy danh sách tất cả task với filter
    /// </summary>
    Task<List<HousekeepingTaskViewModel>> GetAllAsync(
        string? status = null, 
        string? taskType = null, 
        string? priority = null,
        ulong? roomId = null,
        ulong? assignedUserId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null);

    /// <summary>
    /// Lấy task theo ID
    /// </summary>
    Task<HousekeepingTaskViewModel?> GetByIdAsync(ulong id);

    /// <summary>
    /// Lấy danh sách task của user cụ thể (cho Housekeeping staff)
    /// </summary>
    Task<List<HousekeepingTaskViewModel>> GetMyTasksAsync(ulong userId, string? status = null);

    /// <summary>
    /// Lấy task đang Pending của một phòng
    /// </summary>
    Task<HousekeepingTaskViewModel?> GetPendingTaskByRoomAsync(ulong roomId);

    /// <summary>
    /// Tạo task mới
    /// </summary>
    Task<HousekeepingTaskViewModel?> CreateTaskAsync(ulong roomId, string taskType, string priority, DateTime scheduledAt, ulong? assignedUserId = null, string? notes = null, ulong? bookingId = null);

    /// <summary>
    /// Tự động tạo task CheckOut khi booking checkout
    /// </summary>
    Task<HousekeepingTaskViewModel?> AutoCreateCheckoutTaskAsync(ulong bookingId, ulong roomId);

    /// <summary>
    /// Phân công task cho user
    /// </summary>
    Task<bool> AssignTaskAsync(ulong taskId, ulong userId);

    /// <summary>
    /// Cập nhật status task (Start, Complete, Cancel)
    /// </summary>
    Task<bool> UpdateStatusAsync(ulong taskId, string newStatus, string? notes = null);

    /// <summary>
    /// Bắt đầu task (Pending -> InProgress)
    /// </summary>
    Task<bool> StartTaskAsync(ulong taskId, ulong userId);

    /// <summary>
    /// Hoàn thành task (InProgress -> Completed)
    /// </summary>
    Task<bool> CompleteTaskAsync(ulong taskId, string? completionNotes = null);

    /// <summary>
    /// Hủy task
    /// </summary>
    Task<bool> CancelTaskAsync(ulong taskId, string? reason = null);

    /// <summary>
    /// Lấy thống kê housekeeping
    /// </summary>
    Task<(int TotalTasks, int PendingTasks, int InProgressTasks, int CompletedTasks, int OverdueTasks)> GetStatisticsAsync(DateTime? date = null);

    /// <summary>
    /// Lấy trạng thái tất cả phòng (cho room status board)
    /// </summary>
    Task<List<(ulong RoomId, string RoomNumber, string Status, bool HasPendingTask)>> GetRoomStatusBoardAsync(ulong? hotelId = null);

    /// <summary>
    /// Lấy performance của user (số task hoàn thành, thời gian trung bình)
    /// </summary>
    Task<(int CompletedTasks, double AvgCompletionMinutes)> GetUserPerformanceAsync(ulong userId, DateTime? fromDate = null, DateTime? toDate = null);
}
