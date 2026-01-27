using HotelManagement.API.Exceptions;
using HotelManagement.API.Models.DTOs.Common;
using HotelManagement.API.Models.Entities;
using HotelManagement.API.Repositories.Interfaces;
using HotelManagement.API.Services.Interfaces;

namespace HotelManagement.API.Services.Implementations
{
    public class HousekeepingService : IHousekeepingService
    {
        private readonly IUnitOfWork _unitOfWork;

        public HousekeepingService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<HousekeepingTask>> GetAllTasksAsync()
        {
            return await _unitOfWork.HousekeepingTasks.GetAllAsync();
        }

        public async Task<HousekeepingTask?> GetTaskByIdAsync(long id)
        {
            return await _unitOfWork.HousekeepingTasks.GetByIdAsync(id);
        }

        public async Task<IEnumerable<HousekeepingTask>> GetTasksByRoomAsync(long roomId)
        {
            return await _unitOfWork.HousekeepingTasks.GetTasksByRoomAsync(roomId);
        }

        public async Task<IEnumerable<HousekeepingTask>> GetTasksByUserAsync(long userId)
        {
            return await _unitOfWork.HousekeepingTasks.GetTasksByUserAsync(userId);
        }

        public async Task<IEnumerable<HousekeepingTask>> GetTasksByStatusAsync(string status)
        {
            return await _unitOfWork.HousekeepingTasks.GetTasksByStatusAsync(status);
        }

        public async Task<IEnumerable<HousekeepingTask>> GetPendingTasksAsync()
        {
            return await _unitOfWork.HousekeepingTasks.GetPendingTasksAsync();
        }

        public async Task<HousekeepingTask> CreateTaskAsync(HousekeepingTask task)
        {
            // Validate room exists
            var room = await _unitOfWork.Rooms.GetByIdAsync(task.RoomId);
            if (room == null)
                throw new NotFoundException("Room", task.RoomId);

            // Validate assigned user exists if provided
            if (task.AssignedToUserId.HasValue)
            {
                var user = await _unitOfWork.Users.GetByIdAsync(task.AssignedToUserId.Value);
                if (user == null)
                    throw new NotFoundException("User", task.AssignedToUserId.Value);
            }

            task.CreatedAt = DateTime.Now;
            task.Status = task.Status ?? "Pending";

            await _unitOfWork.HousekeepingTasks.AddAsync(task);
            await _unitOfWork.SaveChangesAsync();

            return task;
        }

        public async Task<HousekeepingTask> UpdateTaskAsync(long id, HousekeepingTask task)
        {
            var existingTask = await _unitOfWork.HousekeepingTasks.GetByIdAsync(id);
            if (existingTask == null)
                throw new NotFoundException("HousekeepingTask", id);

            existingTask.TaskType = task.TaskType;
            existingTask.Priority = task.Priority;
            existingTask.Status = task.Status;
            existingTask.Notes = task.Notes;
            existingTask.AssignedToUserId = task.AssignedToUserId;

            await _unitOfWork.HousekeepingTasks.UpdateAsync(existingTask);
            await _unitOfWork.SaveChangesAsync();

            return existingTask;
        }

        public async Task<HousekeepingTask> UpdateTaskStatusAsync(long id, string status)
        {
            var task = await _unitOfWork.HousekeepingTasks.GetByIdAsync(id);
            if (task == null)
                throw new NotFoundException("HousekeepingTask", id);

            var validStatuses = new[] { "Pending", "InProgress", "Completed", "Cancelled" };
            if (!validStatuses.Contains(status))
                throw new ValidationException($"Invalid status. Must be one of: {string.Join(", ", validStatuses)}");

            task.Status = status;

            if (status == "Completed")
            {
                task.CompletedAt = DateTime.Now;
                
                // Update room status to Available when task is completed
                var room = await _unitOfWork.Rooms.GetByIdAsync(task.RoomId);
                if (room != null)
                {
                    room.Status = "Available";
                    await _unitOfWork.Rooms.UpdateAsync(room);
                }
            }

            await _unitOfWork.HousekeepingTasks.UpdateAsync(task);
            await _unitOfWork.SaveChangesAsync();

            return task;
        }

        public async Task<bool> DeleteTaskAsync(long id)
        {
            var task = await _unitOfWork.HousekeepingTasks.GetByIdAsync(id);
            if (task == null)
                throw new NotFoundException("HousekeepingTask", id);

            _unitOfWork.HousekeepingTasks.Delete(task);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> AssignTaskAsync(long taskId, long userId)
        {
            var task = await _unitOfWork.HousekeepingTasks.GetByIdAsync(taskId);
            if (task == null)
                throw new NotFoundException("HousekeepingTask", taskId);

            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
                throw new NotFoundException("User", userId);

            task.AssignedToUserId = userId;
            task.Status = "InProgress";

            await _unitOfWork.HousekeepingTasks.UpdateAsync(task);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<HousekeepingTask> ClaimTaskAsync(long taskId, long userId)
        {
            var task = await _unitOfWork.HousekeepingTasks.GetByIdAsync(taskId);
            if (task == null)
                throw new NotFoundException("HousekeepingTask", taskId);

            // Verify task is pending and unassigned
            if (task.Status != "Pending")
                throw new BusinessException("Only pending tasks can be claimed.");

            if (task.AssignedToUserId.HasValue)
                throw new BusinessException("Task is already assigned to another user.");

            // Verify user exists
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
                throw new NotFoundException("User", userId);

            // Assign to user and update status
            task.AssignedToUserId = userId;
            task.Status = "InProgress";

            await _unitOfWork.HousekeepingTasks.UpdateAsync(task);
            await _unitOfWork.SaveChangesAsync();

            return task;
        }

        public async Task<PagedResult<HousekeepingTask>> GetPagedTasksAsync(int pageNumber, int pageSize, string? status = null)
        {
            var (items, totalCount) = await _unitOfWork.HousekeepingTasks.GetPagedAsync(
                pageNumber,
                pageSize,
                filter: status != null ? t => t.Status == status : null,
                orderBy: query => query.OrderByDescending(t => t.CreatedAt)
            );

            return new PagedResult<HousekeepingTask>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
    }
}
