using System;
using System.Collections.Generic;

namespace HotelManagementSystem.Models.ViewModels.Booking
{
    public class RoomDetailsViewModel
    {
        public long RoomId { get; set; }
        public string RoomNumber { get; set; } = null!;
        public short? Floor { get; set; }
        public string Status { get; set; } = null!;

        // Room Type Info
        public string RoomTypeName { get; set; } = null!;
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public int Capacity { get; set; }
        public decimal BasePrice { get; set; }

        // Hotel Info
        public string HotelName { get; set; } = null!;
        public string? HotelAddress { get; set; }

        // Amenities
        public List<string> Amenities { get; set; } = new List<string>();

        // Search Context (to pass back to Book action)
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public int NumberOfGuests { get; set; }
    }
}
