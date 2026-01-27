namespace HotelManagement.API.Services.Interfaces
{
    public interface INotificationService
    {
        Task SendBookingConfirmationAsync(long bookingId);
        Task SendBookingCancellationAsync(long bookingId);
        Task SendCheckInReminderAsync(long bookingId);
        Task SendCheckOutReminderAsync(long bookingId);
        Task SendPaymentConfirmationAsync(long paymentId);
        Task SendInvoiceAsync(long invoiceId);
        Task SendPasswordResetEmailAsync(string email, string resetToken);
        Task SendWelcomeEmailAsync(long userId);
        Task NotifyHousekeepingTaskAsync(long taskId);
        Task<bool> SendEmailAsync(string to, string subject, string body);
        Task<bool> SendSMSAsync(string phoneNumber, string message);
    }
}
