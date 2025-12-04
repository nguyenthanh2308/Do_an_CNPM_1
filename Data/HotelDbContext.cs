using HotelManagementSystem.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace HotelManagementSystem.Data
{
    public class HotelDbContext : DbContext
    {
        public HotelDbContext(DbContextOptions<HotelDbContext> options)
            : base(options)
        {
        }

        // DbSet cho từng bảng
        public DbSet<User> Users => Set<User>();
        public DbSet<Hotel> Hotels => Set<Hotel>();
        public DbSet<Amenity> Amenities => Set<Amenity>();
        public DbSet<RoomType> RoomTypes => Set<RoomType>();
        public DbSet<RoomTypeAmenity> RoomTypeAmenities => Set<RoomTypeAmenity>();
        public DbSet<Room> Rooms => Set<Room>();
        public DbSet<RatePlan> RatePlans => Set<RatePlan>();
        public DbSet<Promotion> Promotions => Set<Promotion>();
        public DbSet<Guest> Guests => Set<Guest>();
        public DbSet<Booking> Bookings => Set<Booking>();
        public DbSet<BookingRoom> BookingRooms => Set<BookingRoom>();
        public DbSet<Payment> Payments => Set<Payment>();
        public DbSet<Invoice> Invoices => Set<Invoice>();
        public DbSet<HousekeepingTask> HousekeepingTasks => Set<HousekeepingTask>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ========== room_type_amenities (N-N) ==========
            modelBuilder.Entity<RoomTypeAmenity>(entity =>
            {
                // PK composite
                entity.HasKey(e => new { e.RoomTypeId, e.AmenityId });

                entity.HasOne(e => e.RoomType)
                      .WithMany(rt => rt.RoomTypeAmenities)
                      .HasForeignKey(e => e.RoomTypeId)
                      .OnDelete(DeleteBehavior.Cascade);   // ON DELETE CASCADE

                entity.HasOne(e => e.Amenity)
                      .WithMany(a => a.RoomTypeAmenities)
                      .HasForeignKey(e => e.AmenityId)
                      .OnDelete(DeleteBehavior.Restrict);  // ON DELETE RESTRICT
            });

            // ========== Hotel -> RoomTypes / Rooms / Bookings ==========
            modelBuilder.Entity<Hotel>(entity =>
            {
                entity.HasMany(h => h.RoomTypes)
                      .WithOne(rt => rt.Hotel)
                      .HasForeignKey(rt => rt.HotelId)
                      .OnDelete(DeleteBehavior.Restrict);  // fk_roomtypes_hotel

                entity.HasMany(h => h.Rooms)
                      .WithOne(r => r.Hotel)
                      .HasForeignKey(r => r.HotelId)
                      .OnDelete(DeleteBehavior.Restrict);  // fk_rooms_hotel

                entity.HasMany(h => h.Bookings)
                      .WithOne(b => b.Hotel)
                      .HasForeignKey(b => b.HotelId)
                      .OnDelete(DeleteBehavior.Restrict);  // fk_bookings_hotel
            });

            // ========== RoomType -> Rooms / RatePlans ==========
            modelBuilder.Entity<RoomType>(entity =>
            {
                entity.HasMany(rt => rt.Rooms)
                      .WithOne(r => r.RoomType)
                      .HasForeignKey(r => r.RoomTypeId)
                      .OnDelete(DeleteBehavior.Restrict);  // fk_rooms_roomtype

                entity.HasMany(rt => rt.RatePlans)
                      .WithOne(rp => rp.RoomType)
                      .HasForeignKey(rp => rp.RoomTypeId)
                      .OnDelete(DeleteBehavior.Restrict);  // fk_rateplans_roomtype
            });

            // ========== Guest -> Bookings ==========
            modelBuilder.Entity<Guest>(entity =>
            {
                entity.HasMany(g => g.Bookings)
                      .WithOne(b => b.Guest)
                      .HasForeignKey(b => b.GuestId)
                      .OnDelete(DeleteBehavior.Restrict);  // fk_bookings_guest
            });

            // ========== Booking -> BookingRooms / Payments / Invoice (1-1) ==========
            modelBuilder.Entity<Booking>(entity =>
            {
                entity.HasMany(b => b.BookingRooms)
                      .WithOne(br => br.Booking)
                      .HasForeignKey(br => br.BookingId)
                      .OnDelete(DeleteBehavior.Cascade);   // fk_brooms_booking

                entity.HasMany(b => b.Payments)
                      .WithOne(p => p.Booking)
                      .HasForeignKey(p => p.BookingId)
                      .OnDelete(DeleteBehavior.Cascade);   // fk_payments_booking

                // 1-1 Booking - Invoice (uq_invoices_booking)
                entity.HasOne(b => b.Invoice)
                      .WithOne(i => i.Booking)
                      .HasForeignKey<Invoice>(i => i.BookingId)
                      .OnDelete(DeleteBehavior.Cascade);   // fk_invoices_booking
            });

            // ========== BookingRoom -> Room (optional) ==========
            modelBuilder.Entity<BookingRoom>(entity =>
            {
                entity.HasOne(br => br.Room)
                      .WithMany(r => r.BookingRooms)
                      .HasForeignKey(br => br.RoomId)
                      .OnDelete(DeleteBehavior.SetNull);   // fk_brooms_room
            });

            // ========== HousekeepingTask -> Room / User ==========
            modelBuilder.Entity<HousekeepingTask>(entity =>
            {
                entity.HasOne(t => t.Room)
                      .WithMany(r => r.HousekeepingTasks)
                      .HasForeignKey(t => t.RoomId)
                      .OnDelete(DeleteBehavior.Cascade);   // fk_hk_room

                entity.HasOne(t => t.AssignedToUser)
                      .WithMany(u => u.HousekeepingTasks)
                      .HasForeignKey(t => t.AssignedToUserId)
                      .OnDelete(DeleteBehavior.SetNull);   // fk_hk_user

                entity.HasOne(t => t.Booking)
                      .WithMany()
                      .HasForeignKey(t => t.BookingId)
                      .OnDelete(DeleteBehavior.SetNull);   // fk_hk_booking
            });
        }
    }
}
