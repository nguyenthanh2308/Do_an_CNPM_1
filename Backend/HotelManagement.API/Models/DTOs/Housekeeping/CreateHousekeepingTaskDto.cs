using System.ComponentModel.DataAnnotations;

namespace HotelManagement.API.Models.DTOs.Housekeeping
{
    public class CreateHousekeepingTaskDto
    {
        [Required]
        public long RoomId { get; set; }

        public long? AssignedToUserId { get; set; }

        public string TaskType { get; set; } = "Cleaning"; // Cleaning, Maintenance, Inspection, CheckOut

        public string Priority { get; set; } = "Normal"; // Low, Normal, High, Urgent

        public DateTime? ScheduledAt { get; set; }

        public string? Notes { get; set; }
    }
}
