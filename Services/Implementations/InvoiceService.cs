using HotelManagementSystem.Data;
using HotelManagementSystem.Models.Entities;
using HotelManagementSystem.Models.ViewModels.Invoice;
using HotelManagementSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelManagementSystem.Services.Implementations;

public class InvoiceService : IInvoiceService
{
    private readonly HotelDbContext _context;

    public InvoiceService(HotelDbContext context)
    {
        _context = context;
    }

    public async Task<List<InvoiceViewModel>> GetAllAsync(string? status = null, long? bookingId = null, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.Invoices
            .Include(i => i.Booking)
                .ThenInclude(b => b.Guest)
            .Include(i => i.Booking.BookingRooms)
                .ThenInclude(br => br.Room!)
                    .ThenInclude(r => r.RoomType)
            .Include(i => i.Booking.BookingRooms)
                .ThenInclude(br => br.Room!.Hotel)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(i => i.Status == status);
        }

        if (bookingId.HasValue)
        {
            query = query.Where(i => i.BookingId == bookingId.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(i => i.IssuedAt >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(i => i.IssuedAt <= toDate.Value);
        }

        var invoices = await query.OrderByDescending(i => i.CreatedAt).ToListAsync();
        return invoices.Select(MapToViewModel).ToList();
    }

    public async Task<InvoiceViewModel?> GetByIdAsync(long id)
    {
        var invoice = await _context.Invoices
            .Include(i => i.Booking)
                .ThenInclude(b => b.Guest)
            .Include(i => i.Booking.BookingRooms)
                .ThenInclude(br => br.Room!)
                    .ThenInclude(r => r.RoomType)
            .Include(i => i.Booking.BookingRooms)
                .ThenInclude(br => br.Room!.Hotel)
            .FirstOrDefaultAsync(i => i.Id == id);

        return invoice == null ? null : MapToViewModel(invoice);
    }

    public async Task<InvoiceViewModel?> GetByBookingIdAsync(long bookingId)
    {
        var invoice = await _context.Invoices
            .Include(i => i.Booking)
                .ThenInclude(b => b.Guest)
            .Include(i => i.Booking.BookingRooms)
                .ThenInclude(br => br.Room!)
                    .ThenInclude(r => r.RoomType)
            .Include(i => i.Booking.BookingRooms)
                .ThenInclude(br => br.Room!.Hotel)
            .FirstOrDefaultAsync(i => i.BookingId == bookingId);

        return invoice == null ? null : MapToViewModel(invoice);
    }

    public async Task<InvoiceViewModel?> CreateInvoiceAsync(long bookingId, string? notes = null)
    {
        // Kiểm tra booking tồn tại
var booking = await _context.Bookings
            .Include(b => b.Guest)
            .Include(b => b.BookingRooms)
                .ThenInclude(br => br.Room!)
                    .ThenInclude(r => r.RoomType)
            .Include(b => b.BookingRooms)
                .ThenInclude(br => br.Room!.Hotel)
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking == null)
        {
            return null;
        }

        // Kiểm tra đã có invoice chưa
        var existingInvoice = await _context.Invoices.FirstOrDefaultAsync(i => i.BookingId == bookingId);
        if (existingInvoice != null)
        {
            return null; // Booking đã có invoice rồi
        }

        // Generate invoice number
        var invoiceNumber = await GenerateInvoiceNumberAsync();

        // Tính toán các khoản phí
        var roomCharge = booking.TotalAmount;
        var discountAmount = booking.DiscountAmount;
        var finalAmount = roomCharge - discountAmount; // Giá sau giảm
        var taxAmount = Math.Round(finalAmount * 0.1m, 0); // VAT 10%
        var serviceCharge = Math.Round(finalAmount * 0.05m, 0); // Service charge 5%
        var totalAmount = finalAmount + taxAmount + serviceCharge;

        // Tạo invoice mới
        var invoice = new Models.Entities.Invoice
        {
            InvoiceNumber = invoiceNumber,
            BookingId = bookingId,
            Amount = totalAmount,
            IssuedAt = DateTime.Now,
            Status = "Issued", // Tự động phát hành khi tạo
            Notes = notes,
            CreatedAt = DateTime.Now
        };

        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync();

        // Reload để lấy đầy đủ navigation properties
        return await GetByIdAsync(invoice.Id);
    }

    public async Task<string> GenerateInvoiceNumberAsync()
    {
        // Generate unique invoice number using GUID
        return "INV-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
    }

    public async Task<bool> UpdateStatusAsync(long id, string newStatus)
    {
        var invoice = await _context.Invoices.FindAsync(id);
        if (invoice == null)
        {
            return false;
        }

        // Validate status
        var validStatuses = new[] { "Draft", "Issued", "Paid", "Cancelled" };
        if (!validStatuses.Contains(newStatus))
        {
            return false;
        }

        invoice.Status = newStatus;
        invoice.UpdatedAt = DateTime.Now;

        if (newStatus == "Paid")
        {
            invoice.PaidAt = DateTime.Now;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> MarkAsPaidAsync(long id, string? paymentMethod = null)
    {
        var invoice = await _context.Invoices.FindAsync(id);
        if (invoice == null || invoice.Status != "Issued")
        {
            return false;
        }

        invoice.Status = "Paid";
invoice.PaidAt = DateTime.Now;
        invoice.PaymentMethod = paymentMethod;
        invoice.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CancelInvoiceAsync(long id, string? reason = null)
    {
        var invoice = await _context.Invoices.FindAsync(id);
        if (invoice == null || invoice.Status == "Paid")
        {
            return false; // Không thể hủy invoice đã thanh toán
        }

        invoice.Status = "Cancelled";
        if (!string.IsNullOrEmpty(reason))
        {
            invoice.Notes = (invoice.Notes ?? "") + $"\nLý do hủy: {reason}";
        }
        invoice.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<string> GeneratePdfAsync(long id)
    {
        // Mock PDF generation - không implement thật
        // Trong thực tế sẽ dùng thư viện như iTextSharp, DinkToPdf, etc.
        var invoice = await GetByIdAsync(id);
        if (invoice == null)
        {
            return string.Empty;
        }

        // Simulate PDF generation delay
        await Task.Delay(500);

        // Return mock file path
        var fileName = $"{invoice.InvoiceNumber}.pdf";
        var mockPath = $"/invoices/pdf/{fileName}";
        
        return mockPath;
    }

    public async Task<bool> HasInvoiceAsync(long bookingId)
    {
        return await _context.Invoices.AnyAsync(i => i.BookingId == bookingId);
    }

    public async Task<(int TotalCount, decimal TotalAmount, int PaidCount, decimal PaidAmount)> GetStatisticsAsync()
    {
        var invoices = await _context.Invoices.ToListAsync();

        var totalCount = invoices.Count;
        var totalAmount = invoices.Sum(i => i.Amount);
        var paidInvoices = invoices.Where(i => i.Status == "Paid").ToList();
        var paidCount = paidInvoices.Count;
        var paidAmount = paidInvoices.Sum(i => i.Amount);

        return (totalCount, totalAmount, paidCount, paidAmount);
    }

    // Helper method để map Entity sang ViewModel
    private InvoiceViewModel MapToViewModel(Models.Entities.Invoice invoice)
    {
        var booking = invoice.Booking;
        var guest = booking.Guest;
        var room = booking.BookingRooms.FirstOrDefault()?.Room;
        var roomType = room?.RoomType;
        var hotel = room?.Hotel;

        // Tính toán breakdown
        var roomCharge = booking.TotalAmount;
        var discountAmount = booking.DiscountAmount;
        var finalAmount = roomCharge - discountAmount;
        var taxAmount = Math.Round(finalAmount * 0.1m, 0);
        var serviceCharge = Math.Round(finalAmount * 0.05m, 0);

        return new InvoiceViewModel
        {
            Id = (ulong)invoice.Id,
            InvoiceNumber = invoice.InvoiceNumber,
            BookingId = (ulong)invoice.BookingId,
            Amount = invoice.Amount,
            IssuedAt = invoice.IssuedAt,
Status = invoice.Status,

            // Booking info
            BookingCode = $"BK-{booking.Id.ToString("D8")}",
            CheckInDate = booking.CheckInDate,
            CheckOutDate = booking.CheckOutDate,
            Nights = (booking.CheckOutDate - booking.CheckInDate).Days,

            // Guest info
            GuestName = guest.FullName,
            GuestEmail = guest.Email,
            GuestPhone = guest.Phone,
            GuestAddress = null, // Guest entity doesn't have Address field

            // Room info
            RoomNumber = room?.Number ?? "N/A",
            RoomTypeName = roomType?.Name ?? "N/A",
            HotelName = hotel?.Name ?? "N/A",

            // Financial breakdown
            RoomCharge = roomCharge,
            DiscountAmount = discountAmount,
            TaxAmount = taxAmount,
            ServiceCharge = serviceCharge,
            TotalAmount = invoice.Amount,

            // Additional
            Notes = invoice.Notes,
            PaymentMethod = invoice.PaymentMethod,
            PaidAt = invoice.PaidAt,
            CreatedAt = invoice.CreatedAt,
            UpdatedAt = invoice.UpdatedAt
        };
    }
}