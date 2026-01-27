using HotelManagement.API.Models.DTOs.Common;
using HotelManagement.API.Models.Entities;

namespace HotelManagement.API.Services.Interfaces
{
    public interface IHousekeepingService
    {
        Task<IEnumerable<HousekeepingTask>> GetAllTasksAsync();
        Task<HousekeepingTask?> GetTaskByIdAsync(long id);
        Task<IEnumerable<HousekeepingTask>> GetTasksByRoomAsync(long roomId);
        Task<IEnumerable<HousekeepingTask>> GetTasksByUserAsync(long userId);
        Task<IEnumerable<HousekeepingTask>> GetTasksByStatusAsync(string status);
        Task<IEnumerable<HousekeepingTask>> GetPendingTasksAsync();
        Task<HousekeepingTask> CreateTaskAsync(HousekeepingTask task);
        Task<HousekeepingTask> UpdateTaskAsync(long id, HousekeepingTask task);
        Task<HousekeepingTask> UpdateTaskStatusAsync(long id, string status);
        Task<bool> DeleteTaskAsync(long id);
        Task<bool> AssignTaskAsync(long taskId, long userId);
        Task<PagedResult<HousekeepingTask>> GetPagedTasksAsync(int pageNumber, int pageSize, string? status = null);
    }
}
