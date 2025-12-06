using System.Threading.Tasks;

namespace HotelManagementSystem.Services.Interfaces
{
    public interface INotificationService
    {
        Task SendEmailAsync(string to, string subject, string body);
        Task SendSmsAsync(string phoneNumber, string message);
        Task CreateNotificationAsync(long userId, string title, string message, string type = "Info");
    }
}
