// File: Models/Entities/Promotion.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelManagementSystem.Models.Entities
{
    [Table("promotions")]
    public class Promotion
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Required]
        [MaxLength(32)]
        [Column("code")]
        public string Code { get; set; } = null!;

        // ENUM('Percent','Amount')
        [Required]
        [Column("type")]
        public string Type { get; set; } = null!;

        [Required]
        [Column("value")]
        public decimal Value { get; set; }

        [Required]
        [Column("start_date")]
        public DateTime StartDate { get; set; }

        [Required]
        [Column("end_date")]
        public DateTime EndDate { get; set; }

        [Column("conditions_json")]
        public string? ConditionsJson { get; set; }

        [Required]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}
