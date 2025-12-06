using System;
using System.Threading.Tasks;
using HotelManagementSystem.Services.Interfaces;

namespace HotelManagementSystem.Services.Implementations
{
    public class NotificationService : INotificationService
    {
        // In a real application, you would inject email/SMS providers here (e.g., SendGrid, Twilio)
        
        public async Task SendEmailAsync(string to, string subject, string body)
        {
            // Simulate sending email
            await Task.Delay(100); // Simulate network delay
            Console.WriteLine($"[Email Sent] To: {to}, Subject: {subject}");
        }

        public async Task SendSmsAsync(string phoneNumber, string message)
        {
            // Simulate sending SMS
            await Task.Delay(100);
            Console.WriteLine($"[SMS Sent] To: {phoneNumber}, Message: {message}");
        }

        public async Task CreateNotificationAsync(long userId, string title, string message, string type = "Info")
        {
            // Simulate creating in-app notification
            // You would typically save this to a Notifications table in the database
            await Task.Delay(50);
            Console.WriteLine($"[Notification Created] User: {userId}, Title: {title}, Message: {message}");
        }
    }
}
