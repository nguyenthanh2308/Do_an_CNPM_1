using HotelManagement.API.Models.Entities;

namespace HotelManagement.API.Services.Interfaces
{
    public interface INotificationService
    {
        // Booking-related notifications
        Task SendBookingConfirmationAsync(long bookingId);
        Task SendBookingCancellationAsync(long bookingId);
        Task SendCheckInReminderAsync(long bookingId);
        Task SendCheckOutReminderAsync(long bookingId);
        
        // Payment & Invoice notifications
        Task SendPaymentConfirmationAsync(long paymentId);
        Task SendInvoiceAsync(long invoiceId);
        
        // User notifications
        Task SendPasswordResetEmailAsync(string email, string resetToken);
        Task SendWelcomeEmailAsync(long userId);
        
        // Housekeeping notifications
        Task NotifyHousekeepingTaskAsync(long taskId);
        
        // Communication channels
        Task<bool> SendEmailAsync(string to, string subject, string body);
        Task<bool> SendSMSAsync(string phoneNumber, string message);
        
        // Notification Management (for NotificationsController)
        Task<IEnumerable<Notification>> GetUserNotificationsAsync(long userId);
        Task<Notification?> GetNotificationByIdAsync(long id);
        Task MarkAsReadAsync(long notificationId);
        Task MarkAllAsReadAsync(long userId);
        Task DeleteNotificationAsync(long notificationId);
        Task SendBroadcastNotificationAsync(string message, string targetRole);
    }
}
