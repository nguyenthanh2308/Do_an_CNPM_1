using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using HotelManagementSystem.Models.Entities;

namespace HotelManagementSystem.Models.ViewModels.Booking
{
    public class BookingSearchViewModel
    {
        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Check-in date")]
        public DateTime? CheckInDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Check-out date")]
        public DateTime? CheckOutDate { get; set; }

        [Required]
        [Range(1, 20)]
        [Display(Name = "Number of guests")]
        public int NumberOfGuests { get; set; } = 1;

        // Optional: filter theo hotel / room type nếu sau này cần
        public long? HotelId { get; set; }
        public long? RoomTypeId { get; set; }

        // Kết quả tìm kiếm
        public List<Room>? AvailableRooms { get; set; }
    }
}
