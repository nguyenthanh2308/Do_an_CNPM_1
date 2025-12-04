// File: Models/Entities/HousekeepingTask.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelManagementSystem.Models.Entities
{
    [Table("housekeeping_tasks")]
    public class HousekeepingTask
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Required]
        [Column("room_id")]
        public long RoomId { get; set; }

        [Column("assigned_to_user_id")]
        public long? AssignedToUserId { get; set; }

        [Required]
        [MaxLength(20)]
        [Column("task_type")]
        public string TaskType { get; set; } = "Cleaning"; // Cleaning, Maintenance, Inspection, CheckOut

        [Required]
        [MaxLength(20)]
        [Column("status")]
        public string Status { get; set; } = "Pending"; // Pending, InProgress, Completed, Cancelled

        [Required]
        [MaxLength(20)]
        [Column("priority")]
        public string Priority { get; set; } = "Normal"; // Low, Normal, High, Urgent

        [Required]
        [Column("scheduled_at")]
        public DateTime ScheduledAt { get; set; } = DateTime.Now;

        [Column("completed_at")]
        public DateTime? CompletedAt { get; set; }

        [Column("notes", TypeName = "text")]
        public string? Notes { get; set; }

        // Link to booking (if CheckOut task)
        [Column("booking_id")]
        public long? BookingId { get; set; }

        [Required]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Backward compatibility
        [NotMapped]
        public DateTime? DueTime
        {
            get => ScheduledAt;
            set { if (value.HasValue) ScheduledAt = value.Value; }
        }

        [ForeignKey(nameof(RoomId))]
        public virtual Room Room { get; set; } = null!;

        [ForeignKey(nameof(AssignedToUserId))]
        public virtual User? AssignedToUser { get; set; }

        [ForeignKey(nameof(BookingId))]
        public virtual Booking? Booking { get; set; }
    }
}
