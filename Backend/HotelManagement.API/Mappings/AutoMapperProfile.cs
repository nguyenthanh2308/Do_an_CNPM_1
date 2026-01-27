using AutoMapper;
using HotelManagement.API.Models.Entities;
using HotelManagement.API.Models.DTOs.Auth;
using HotelManagement.API.Models.DTOs.Booking;
using HotelManagement.API.Models.DTOs.Room;
using HotelManagement.API.Models.DTOs.Guest;
using HotelManagement.API.Models.DTOs.Payment;
using HotelManagement.API.Models.DTOs.Hotel;

namespace HotelManagement.API.Mappings;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        // ========== User Mappings ==========
        CreateMap<User, UserDto>();
        CreateMap<RegisterRequestDto, User>()
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore()) // Will be set in service
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.Now))
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => "Customer")); // Default role

        // ========== Booking Mappings ==========
        CreateMap<Booking, BookingDto>()
            .ForMember(dest => dest.HotelName, opt => opt.MapFrom(src => src.Hotel != null ? src.Hotel.Name : string.Empty))
            .ForMember(dest => dest.GuestName, opt => opt.MapFrom(src => src.Guest != null ? src.Guest.FullName : string.Empty));

        CreateMap<Booking, BookingDetailDto>()
            .ForMember(dest => dest.Rooms, opt => opt.MapFrom(src => 
                src.BookingRooms != null ? src.BookingRooms.Select(br => br.Room).ToList() : new List<Entities.Room>()))
            .ForMember(dest => dest.Payments, opt => opt.MapFrom(src => 
                src.Payments != null ? src.Payments : new List<Payment>()));

        CreateMap<CreateBookingDto, Booking>()
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.Now))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => "Pending"))
            .ForMember(dest => dest.PaymentStatus, opt => opt.MapFrom(src => "Unpaid"))
            .ForMember(dest => dest.BookingRooms, opt => opt.Ignore()) // Will be set manually in service
            .ForMember(dest => dest.TotalAmount, opt => opt.Ignore()); // Will be calculated in service

        // ========== Room Mappings ==========
        CreateMap<Entities.Room, RoomDto>()
            .ForMember(dest => dest.RoomTypeName, opt => opt.MapFrom(src => src.RoomType != null ? src.RoomType.Name : string.Empty))
            .ForMember(dest => dest.HotelName, opt => opt.MapFrom(src => src.Hotel != null ? src.Hotel.Name : string.Empty));

        CreateMap<CreateRoomDto, Entities.Room>()
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.Now));

        // ========== Guest Mappings ==========
        CreateMap<Guest, GuestDto>();
        CreateMap<CreateGuestDto, Guest>()
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.Now));

        // ========== Payment Mappings ==========
        CreateMap<Payment, PaymentDto>();

        // ========== Invoice Mappings ==========
        CreateMap<Invoice, InvoiceDto>();

        // ========== Hotel Mappings ==========
        CreateMap<Entities.Hotel, HotelDto>();
    }
}
