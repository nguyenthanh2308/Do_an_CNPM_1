using System;
using HotelManagementSystem.Models.ViewModels.Hotel;
using HotelManagementSystem.Models.ViewModels.RoomType;

namespace HotelManagementSystem.Models.ViewModels.Booking
{
    public class RoomViewModel
    {
        public long Id { get; set; }
        public string Number { get; set; } = null!;
        public int Floor { get; set; }
        public string Status { get; set; } = null!;
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }

        public HotelViewModel Hotel { get; set; } = null!;
        public RoomTypeViewModel RoomType { get; set; } = null!;
    }
}
