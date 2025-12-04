using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotelManagementSystem.Data;
using HotelManagementSystem.Models.Entities;
using HotelManagementSystem.Models.ViewModels.Guest;
using HotelManagementSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelManagementSystem.Services.Implementations
{
    public class GuestService : IGuestService
    {
        private readonly HotelDbContext _context;

        public GuestService(HotelDbContext context)
        {
            _context = context;
        }

        public async Task<List<GuestViewModel>> GetAllAsync()
        {
            var guests = await _context.Guests
                .Include(g => g.Bookings)
                .OrderByDescending(g => g.CreatedAt)
                .ToListAsync();

            return guests.Select(MapToViewModel).ToList();
        }

        public async Task<GuestViewModel?> GetByIdAsync(long id)
        {
            var guest = await _context.Guests
                .Include(g => g.Bookings)
                .FirstOrDefaultAsync(g => g.Id == id);

            return guest == null ? null : MapToViewModel(guest);
        }

        public async Task<List<GuestViewModel>> SearchAsync(string? keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return await GetAllAsync();

            keyword = keyword.Trim().ToLower();

            var guests = await _context.Guests
                .Include(g => g.Bookings)
                .Where(g => g.FullName.ToLower().Contains(keyword) ||
                           (g.Email != null && g.Email.ToLower().Contains(keyword)) ||
                           (g.Phone != null && g.Phone.Contains(keyword)) ||
                           (g.IdNumber != null && g.IdNumber.Contains(keyword)))
                .OrderByDescending(g => g.CreatedAt)
                .ToListAsync();

            return guests.Select(MapToViewModel).ToList();
        }

        public async Task<bool> CreateAsync(GuestViewModel model)
        {
            try
            {
                // Validate email uniqueness
                if (!string.IsNullOrWhiteSpace(model.Email))
                {
                    if (await IsEmailExistsAsync(model.Email))
                        return false;
                }

                var guest = new Guest
                {
                    FullName = model.FullName.Trim(),
                    Email = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email.Trim().ToLower(),
                    Phone = string.IsNullOrWhiteSpace(model.Phone) ? null : model.Phone.Trim(),
                    IdNumber = string.IsNullOrWhiteSpace(model.IdNumber) ? null : model.IdNumber.Trim(),
                    CreatedAt = DateTime.Now
                };

                _context.Guests.Add(guest);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateAsync(GuestViewModel model)
        {
            try
            {
                var guest = await _context.Guests.FindAsync(model.Id);
                if (guest == null)
                    return false;

                // Validate email uniqueness (exclude current)
                if (!string.IsNullOrWhiteSpace(model.Email))
                {
                    if (await IsEmailExistsAsync(model.Email, model.Id))
                        return false;
                }

                guest.FullName = model.FullName.Trim();
                guest.Email = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email.Trim().ToLower();
                guest.Phone = string.IsNullOrWhiteSpace(model.Phone) ? null : model.Phone.Trim();
                guest.IdNumber = string.IsNullOrWhiteSpace(model.IdNumber) ? null : model.IdNumber.Trim();

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteAsync(long id)
        {
            try
            {
                var guest = await _context.Guests
                    .Include(g => g.Bookings)
                    .FirstOrDefaultAsync(g => g.Id == id);

                if (guest == null)
                    return false;

                // Don't allow delete if guest has bookings
                if (guest.Bookings.Any())
                    return false;

                _context.Guests.Remove(guest);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> IsEmailExistsAsync(string email, long? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            email = email.Trim().ToLower();

            var query = _context.Guests.Where(g => g.Email == email);

            if (excludeId.HasValue)
            {
                query = query.Where(g => g.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<GuestViewModel?> FindByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;

            email = email.Trim().ToLower();

            var guest = await _context.Guests
                .Include(g => g.Bookings)
                .FirstOrDefaultAsync(g => g.Email == email);

            return guest == null ? null : MapToViewModel(guest);
        }

        public async Task<GuestViewModel?> FindByPhoneAsync(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return null;

            phone = phone.Trim();

            var guest = await _context.Guests
                .Include(g => g.Bookings)
                .FirstOrDefaultAsync(g => g.Phone == phone);

            return guest == null ? null : MapToViewModel(guest);
        }

        public async Task<GuestViewModel?> FindByIdNumberAsync(string idNumber)
        {
            if (string.IsNullOrWhiteSpace(idNumber))
                return null;

            idNumber = idNumber.Trim();

            var guest = await _context.Guests
                .Include(g => g.Bookings)
                .FirstOrDefaultAsync(g => g.IdNumber == idNumber);

            return guest == null ? null : MapToViewModel(guest);
        }

        // Private helper methods
        private GuestViewModel MapToViewModel(Guest guest)
        {
            var viewModel = new GuestViewModel
            {
                Id = guest.Id,
                FullName = guest.FullName,
                Email = guest.Email,
                Phone = guest.Phone,
                IdNumber = guest.IdNumber,
                CreatedAt = guest.CreatedAt
            };

            // Calculate statistics from bookings
            if (guest.Bookings != null && guest.Bookings.Any())
            {
                viewModel.TotalBookings = guest.Bookings.Count;
                viewModel.CompletedBookings = guest.Bookings.Count(b => b.Status == "CheckedOut");
                viewModel.CancelledBookings = guest.Bookings.Count(b => b.Status == "Cancelled");
                viewModel.TotalSpent = guest.Bookings
                    .Where(b => b.Status != "Cancelled" && b.PaymentStatus == "Paid")
                    .Sum(b => b.TotalAmount);
                viewModel.LastBookingDate = guest.Bookings.Max(b => b.CreatedAt);
            }

            return viewModel;
        }
    }
}
