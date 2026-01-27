using AutoMapper;
using HotelManagement.API.Models.Entities;
using HotelManagement.API.Models.DTOs.Auth;
using HotelManagement.API.Models.DTOs.Booking;
using HotelManagement.API.Models.DTOs.Room;
using HotelManagement.API.Models.DTOs.Guest;
using HotelManagement.API.Models.DTOs.Payment;
using HotelManagement.API.Models.DTOs.Hotel;
using HotelManagement.API.Models.DTOs.Amenity;
using HotelManagement.API.Models.DTOs.RoomType;

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
                src.BookingRooms != null ? src.BookingRooms.Select(br => br.Room).ToList() : new List<Room>()))
            .ForMember(dest => dest.Payments, opt => opt.MapFrom(src => 
                src.Payments != null ? src.Payments : new List<Payment>()));

        CreateMap<CreateBookingDto, Booking>()
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.Now))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => "Pending"))
            .ForMember(dest => dest.PaymentStatus, opt => opt.MapFrom(src => "Unpaid"))
            .ForMember(dest => dest.BookingRooms, opt => opt.Ignore()) // Will be set manually in service
            .ForMember(dest => dest.TotalAmount, opt => opt.Ignore()); // Will be calculated in service

        // ========== Room Mappings ==========
        CreateMap<Room, RoomDto>()
            .ForMember(dest => dest.RoomNumber, opt => opt.MapFrom(src => src.Number))
            .ForMember(dest => dest.RoomTypeName, opt => opt.MapFrom(src => src.RoomType != null ? src.RoomType.Name : string.Empty))
            .ForMember(dest => dest.HotelName, opt => opt.MapFrom(src => src.Hotel != null ? src.Hotel.Name : string.Empty))
            .ForMember(dest => dest.BasePrice, opt => opt.MapFrom(src => src.RoomType != null ? src.RoomType.BasePrice : 0))
            .ForMember(dest => dest.Floor, opt => opt.MapFrom(src => src.Floor ?? 0))
            .ForMember(dest => dest.ViewType, opt => opt.Ignore());

        CreateMap<CreateRoomDto, Room>()
            .ForMember(dest => dest.Number, opt => opt.MapFrom(src => src.RoomNumber))
            .ForMember(dest => dest.Floor, opt => opt.MapFrom(src => (short?)src.Floor))
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
        CreateMap<Hotel, HotelDto>();
        CreateMap<CreateHotelDto, Hotel>()
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.Now));
        CreateMap<UpdateHotelDto, Hotel>();

        // ========== Amenity Mappings ==========
        CreateMap<Amenity, AmenityDto>();
        CreateMap<CreateAmenityDto, Amenity>();

        // ========== RoomType Mappings ==========
        CreateMap<RoomType, RoomTypeDto>()
            .ForMember(dest => dest.HotelName, opt => opt.MapFrom(src => src.Hotel != null ? src.Hotel.Name : string.Empty))
            .ForMember(dest => dest.Amenities, opt => opt.MapFrom(src => 
                src.RoomTypeAmenities != null ? src.RoomTypeAmenities.Select(rta => rta.Amenity.Name).ToList() : new List<string>()));
        CreateMap<CreateRoomTypeDto, RoomType>()
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.Now))
            .ForMember(dest => dest.RoomTypeAmenities, opt => opt.Ignore());
        CreateMap<UpdateRoomTypeDto, RoomType>()
            .ForMember(dest => dest.RoomTypeAmenities, opt => opt.Ignore());

        // ========== Payment/CreateDto Mapping ==========
        CreateMap<CreatePaymentDto, Payment>()
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.Now))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => "Pending"));
    }
}
