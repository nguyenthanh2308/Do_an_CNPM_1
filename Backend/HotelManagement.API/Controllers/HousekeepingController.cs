using HotelManagement.API.Exceptions;
using HotelManagement.API.Models.DTOs.Common;
using HotelManagement.API.Models.DTOs.Housekeeping;
using HotelManagement.API.Models.Entities;
using HotelManagement.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class HousekeepingController : ControllerBase
    {
        private readonly IHousekeepingService _housekeepingService;
        private readonly ILogger<HousekeepingController> _logger;

        public HousekeepingController(IHousekeepingService housekeepingService, ILogger<HousekeepingController> logger)
        {
            _housekeepingService = housekeepingService;
            _logger = logger;
        }

        [HttpGet("my-tasks")]
        [Authorize(Roles = "Housekeeping")]
        public async Task<IActionResult> GetMyTasks()
        {
            try
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (!long.TryParse(userIdClaim, out var userId))
                {
                    return BadRequest(new ApiResponse<object> { Success = false, Message = "Invalid user ID" });
                }

                var tasks = await _housekeepingService.GetTasksByUserAsync(userId);
                return Ok(new ApiResponse<IEnumerable<HousekeepingTask>> { Success = true, Data = tasks });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get my tasks");
                return StatusCode(500, new ApiResponse<object> { Success = false, Message = "An error occurred" });
            }
        }

        [HttpGet("pending")]
        [Authorize(Roles = "Housekeeping,Receptionist,Manager")]
        public async Task<IActionResult> GetPendingTasks()
        {
            try
            {
                var tasks = await _housekeepingService.GetPendingTasksAsync();
                return Ok(new ApiResponse<IEnumerable<HousekeepingTask>> { Success = true, Data = tasks });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get pending tasks");
                return StatusCode(500, new ApiResponse<object> { Success = false, Message = "An error occurred" });
            }
        }

        [HttpPost("{taskId}/claim")]
        [Authorize(Roles = "Housekeeping")]
        public async Task<IActionResult> ClaimTask(long taskId)
        {
            try
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (!long.TryParse(userIdClaim, out var userId))
                {
                    return BadRequest(new ApiResponse<object> { Success = false, Message = "Invalid user ID" });
                }

                var task = await _housekeepingService.ClaimTaskAsync(taskId, userId);
                return Ok(new ApiResponse<HousekeepingTask> { Success = true, Message = "Task claimed successfully", Data = task });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new ApiResponse<object> { Success = false, Message = ex.Message });
            }
            catch (BusinessException ex)
            {
                return BadRequest(new ApiResponse<object> { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to claim task {TaskId}", taskId);
                return StatusCode(500, new ApiResponse<object> { Success = false, Message = "An error occurred" });
            }
        }

        [HttpPut("{taskId}/status")]
        [Authorize(Roles = "Housekeeping,Manager")]
        public async Task<IActionResult> UpdateTaskStatus(long taskId, [FromBody] UpdateTaskStatusDto dto)
        {
            try
            {
                var task = await _housekeepingService.UpdateTaskStatusAsync(taskId, dto.Status);
                return Ok(new ApiResponse<HousekeepingTask> { Success = true, Message = "Task status updated successfully", Data = task });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new ApiResponse<object> { Success = false, Message = ex.Message });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new ApiResponse<object> { Success = false, Message = ex.Message, Errors = ex.Errors });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update task status {TaskId}", taskId);
                return StatusCode(500, new ApiResponse<object> { Success = false, Message = "An error occurred" });
            }
        }

        [HttpPost("{taskId}/notes")]
        [Authorize(Roles = "Housekeeping,Manager")]
        public async Task<IActionResult> AddTaskNotes(long taskId, [FromBody] AddTaskNotesDto dto)
        {
            try
            {
                var existingTask = await _housekeepingService.GetTaskByIdAsync(taskId);
                if (existingTask == null)
                {
                    return NotFound(new ApiResponse<object> { Success = false, Message = $"Task with ID {taskId} not found" });
                }

                existingTask.Notes = (existingTask.Notes ?? "") + "\n" + dto.Notes;
                var task = await _housekeepingService.UpdateTaskAsync(taskId, existingTask);
                
                return Ok(new ApiResponse<HousekeepingTask> { Success = true, Message = "Notes added successfully", Data = task });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add notes to task {TaskId}", taskId);
                return StatusCode(500, new ApiResponse<object> { Success = false, Message = "An error occurred" });
            }
        }

        [HttpPost("{taskId}/report-issues")]
        [Authorize(Roles = "Housekeeping")]
        public async Task<IActionResult> ReportIssues(long taskId, [FromBody] ReportIssuesDto dto)
        {
            try
            {
                var existingTask = await _housekeepingService.GetTaskByIdAsync(taskId);
                if (existingTask == null)
                {
                    return NotFound(new ApiResponse<object> { Success = false, Message = $"Task with ID {taskId} not found" });
                }

                existingTask.Notes = (existingTask.Notes ?? "") + "\n[ISSUE] " + dto.Issues;
                var task = await _housekeepingService.UpdateTaskAsync(taskId, existingTask);
                
                return Ok(new ApiResponse<HousekeepingTask> { Success = true, Message = "Issues reported successfully", Data = task });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to report issues for task {TaskId}", taskId);
                return StatusCode(500, new ApiResponse<object> { Success = false, Message = "An error occurred" });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Receptionist,Manager")]
        public async Task<IActionResult> CreateTask([FromBody] CreateHousekeepingTaskDto dto)
        {
            try
            {
                var task = new HousekeepingTask
                {
                    RoomId = dto.RoomId,
                    AssignedToUserId = dto.AssignedToUserId,
                    TaskType = dto.TaskType,
                    Priority = dto.Priority,
                    ScheduledAt = dto.ScheduledAt ?? DateTime.UtcNow,
                    Notes = dto.Notes,
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow
                };
                var createdTask = await _housekeepingService.CreateTaskAsync(task);
                return CreatedAtAction(nameof(GetTaskById), new { id = createdTask.Id }, new ApiResponse<HousekeepingTask> { Success = true, Message = "Housekeeping task created successfully", Data = createdTask });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new ApiResponse<object> { Success = false, Message = ex.Message, Errors = ex.Errors });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create housekeeping task");
                return StatusCode(500, new ApiResponse<object> { Success = false, Message = "An error occurred" });
            }
        }

        [HttpPost("{taskId}/assign/{userId}")]
        [Authorize(Roles = "Receptionist,Manager")]
        public async Task<IActionResult> AssignTask(long taskId, long userId)
        {
            try
            {
                var success = await _housekeepingService.AssignTaskAsync(taskId, userId);
                if (!success)
                {
                    return BadRequest(new ApiResponse<object> { Success = false, Message = "Failed to assign task" });
                }
                
                var task = await _housekeepingService.GetTaskByIdAsync(taskId);
                return Ok(new ApiResponse<HousekeepingTask> { Success = true, Message = "Task assigned successfully", Data = task });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new ApiResponse<object> { Success = false, Message = ex.Message });
            }
            catch (BusinessException ex)
            {
                return BadRequest(new ApiResponse<object> { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to assign task {TaskId} to user {UserId}", taskId, userId);
                return StatusCode(500, new ApiResponse<object> { Success = false, Message = "An error occurred" });
            }
        }

        [HttpGet]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> GetAllTasks([FromQuery] string? status = null)
        {
            try
            {
                IEnumerable<HousekeepingTask> tasks;
                
                if (!string.IsNullOrEmpty(status))
                {
                    tasks = await _housekeepingService.GetTasksByStatusAsync(status);
                }
                else
                {
                    tasks = await _housekeepingService.GetAllTasksAsync();
                }

                return Ok(new ApiResponse<IEnumerable<HousekeepingTask>> { Success = true, Data = tasks });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get all tasks");
                return StatusCode(500, new ApiResponse<object> { Success = false, Message = "An error occurred" });
            }
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Housekeeping,Receptionist,Manager")]
        public async Task<IActionResult> GetTaskById(long id)
        {
            try
            {
                var task = await _housekeepingService.GetTaskByIdAsync(id);
                if (task == null)
                {
                    return NotFound(new ApiResponse<object> { Success = false, Message = $"Task with ID {id} not found" });
                }

                return Ok(new ApiResponse<HousekeepingTask> { Success = true, Data = task });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get task {TaskId}", id);
                return StatusCode(500, new ApiResponse<object> { Success = false, Message = "An error occurred" });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> DeleteTask(long id)
        {
            try
            {
                var success = await _housekeepingService.DeleteTaskAsync(id);
                if (success)
                {
                    return Ok(new ApiResponse<object> { Success = true, Message = "Housekeeping task deleted successfully" });
                }
                else
                {
                    return BadRequest(new ApiResponse<object> { Success = false, Message = "Failed to delete task" });
                }
            }
            catch (NotFoundException ex)
            {
                return NotFound(new ApiResponse<object> { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete task {TaskId}", id);
                return StatusCode(500, new ApiResponse<object> { Success = false, Message = "An error occurred" });
            }
        }
    }
}
