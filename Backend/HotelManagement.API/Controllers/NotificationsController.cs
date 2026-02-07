using HotelManagement.API.Models.Entities;
using HotelManagement.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HotelManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<NotificationsController> _logger;

    public NotificationsController(
        INotificationService notificationService,
        ILogger<NotificationsController> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    // GET: api/notifications/my-notifications
    [HttpGet("my-notifications")]
    [Authorize]
    public async Task<IActionResult> GetMyNotifications()
    {
        try
        {
            var userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var notifications = await _notificationService.GetUserNotificationsAsync(userId);
            return Ok(new { success = true, data = notifications });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user notifications");
            return StatusCode(500, new { success = false, message = "Failed to retrieve notifications" });
        }
    }

    // GET: api/notifications/unread-count
    [HttpGet("unread-count")]
    [Authorize]
    public async Task<IActionResult> GetUnreadCount()
    {
        try
        {
            var userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var notifications = await _notificationService.GetUserNotificationsAsync(userId);
            var unreadCount = notifications.Count(n => !n.IsRead);
            return Ok(new { success = true, data = new { count = unreadCount } });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unread count");
            return StatusCode(500, new { success = false, message = "Failed to get unread count" });
        }
    }

    // PUT: api/notifications/{id}/read
    [HttpPut("{id}/read")]
    [Authorize]
    public async Task<IActionResult> MarkAsRead(long id)
    {
        try
        {
            var userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            
            // Verify notification belongs to current user
            var notification = await _notificationService.GetNotificationByIdAsync(id);
            if (notification == null)
                return NotFound(new { success = false, message = "Notification not found" });

            if (notification.UserId != userId)
                return Forbid();

            await _notificationService.MarkAsReadAsync(id);
            return Ok(new { success = true, message = "Notification marked as read" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking notification as read");
            return StatusCode(500, new { success = false, message = "Failed to mark notification as read" });
        }
    }

    // PUT: api/notifications/mark-all-read
    [HttpPut("mark-all-read")]
    [Authorize]
    public async Task<IActionResult> MarkAllAsRead()
    {
        try
        {
            var userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            await _notificationService.MarkAllAsReadAsync(userId);
            return Ok(new { success = true, message = "All notifications marked as read" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking all notifications as read");
            return StatusCode(500, new { success = false, message = "Failed to mark all as read" });
        }
    }

    // POST: api/notifications/broadcast (Manager only)
    [HttpPost("broadcast")]
    [Authorize(Roles = "Manager,Admin")]
    public async Task<IActionResult> BroadcastNotification([FromBody] BroadcastNotificationRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Message))
                return BadRequest(new { success = false, message = "Message is required" });

            await _notificationService.SendBroadcastNotificationAsync(
                request.Message,
                request.TargetRole ?? "All"
            );

            return Ok(new { success = true, message = "Broadcast sent successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting notification");
            return StatusCode(500, new { success = false, message = "Failed to send broadcast" });
        }
    }

    // DELETE: api/notifications/{id}
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteNotification(long id)
    {
        try
        {
            var userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            
            var notification = await _notificationService.GetNotificationByIdAsync(id);
            if (notification == null)
                return NotFound(new { success = false, message = "Notification not found" });

            if (notification.UserId != userId)
                return Forbid();

            await _notificationService.DeleteNotificationAsync(id);
            return Ok(new { success = true, message = "Notification deleted" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting notification");
            return StatusCode(500, new { success = false, message = "Failed to delete notification" });
        }
    }
}

// Request DTOs
public class BroadcastNotificationRequest
{
    public string Message { get; set; } = string.Empty;
    public string? TargetRole { get; set; }
}
