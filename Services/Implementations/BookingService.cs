using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using HotelManagementSystem.Data;
using HotelManagementSystem.Models.Entities;
using HotelManagementSystem.Models.ViewModels.Booking;
using HotelManagementSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelManagementSystem.Services.Implementations
{
    public class BookingService : IBookingService
    {
        private readonly HotelDbContext _dbContext;

        public BookingService(HotelDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<long> CreateBookingAsync(
            long hotelId,
            long guestId,
            long roomId,
            long ratePlanId,
            DateTime checkInDate,
            DateTime checkOutDate,
            int numberOfGuests)
        {
            if (checkInDate.Date >= checkOutDate.Date)
                throw new ArgumentException("Check-in date must be earlier than check-out date.");

            if (numberOfGuests <= 0)
                throw new ArgumentException("Number of guests must be greater than zero.");

            // Transaction đảm bảo các bước là atomic
            await using var tx = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                // 1. Load Room + RoomType (để kiểm tra capacity)
                var room = await _dbContext.Rooms
                    .Include(r => r.RoomType)
                    .FirstOrDefaultAsync(r => r.Id == roomId && r.HotelId == hotelId);

                if (room == null)
                    throw new InvalidOperationException("Room not found for given hotel.");

                if (room.RoomType == null || room.RoomType.Capacity < numberOfGuests)
                    throw new InvalidOperationException("Room capacity is not enough for the number of guests.");

                // 2. Load RatePlan (đảm bảo cùng RoomType và còn hiệu lực theo ngày)
                var ratePlan = await _dbContext.RatePlans
                    .FirstOrDefaultAsync(rp =>
                        rp.Id == ratePlanId &&
                        rp.RoomTypeId == room.RoomTypeId &&
                        checkInDate.Date >= rp.StartDate &&
                        checkOutDate.Date <= rp.EndDate);

                if (ratePlan == null)
                    throw new InvalidOperationException("Rate plan not valid for given room type or dates.");

                // 3. Kiểm tra lại xem phòng còn trống không (recheck availability)
                var hasConflict = await _dbContext.BookingRooms
                    .AnyAsync(br =>
                        br.RoomId == roomId &&
                        br.Booking != null &&
                        (br.Booking.Status == "Confirmed" || br.Booking.Status == "CheckedIn") &&
                        br.Booking.CheckInDate < checkOutDate &&
                        br.Booking.CheckOutDate > checkInDate
                    );

                if (hasConflict)
                    throw new InvalidOperationException("Room is no longer available for the selected dates.");

                // 4. Tính toán tổng tiền dựa trên RatePlan.Price
                var nights = (checkOutDate.Date - checkInDate.Date).Days;
                if (nights <= 0)
                    throw new InvalidOperationException("Invalid nights calculation.");

                var pricePerNight = ratePlan.Price; // có thể áp dụng weekend_rule_json sau
                var totalAmount = pricePerNight * nights;

                // 5. Tạo Booking
                var ratePlanSnapshot = new
                {
                    ratePlan.Id,
                    ratePlan.Name,
                    ratePlan.Type,
                    ratePlan.Price,
                    ratePlan.StartDate,
                    ratePlan.EndDate,
                    ratePlan.FreeCancelUntilHours
                };

                var booking = new Booking
                {
                    HotelId = hotelId,
                    GuestId = guestId,
                    CheckInDate = checkInDate.Date,
                    CheckOutDate = checkOutDate.Date,
                    Status = "Confirmed",     // sau khi đặt xong
                    TotalAmount = totalAmount,
                    PaymentStatus = "Unpaid", // thanh toán mô phỏng sau
                    RatePlanSnapshotJson = JsonSerializer.Serialize(ratePlanSnapshot),
                    CreatedAt = DateTime.UtcNow
                };

                // Gắn BookingRoom (1 booking – 1 room)
                var bookingRoom = new BookingRoom
                {
                    Booking = booking,
                    RoomId = roomId,
                    PricePerNight = pricePerNight,
                    Nights = nights
                };

                _dbContext.Bookings.Add(booking);
                _dbContext.BookingRooms.Add(bookingRoom);

                await _dbContext.SaveChangesAsync();

                // Commit transaction
                await tx.CommitAsync();

                return booking.Id;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task<List<BookingViewModel>> GetAllAsync(
            string? status = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            long? guestId = null,
            long? hotelId = null)
        {
            var query = _dbContext.Bookings
                .Include(b => b.Guest)
                .Include(b => b.Hotel)
                .Include(b => b.Promotion)
                .Include(b => b.BookingRooms)
                    .ThenInclude(br => br.Room)
                        .ThenInclude(r => r.RoomType)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
                query = query.Where(b => b.Status == status);

            if (fromDate.HasValue)
                query = query.Where(b => b.CheckInDate >= fromDate.Value.Date);

            if (toDate.HasValue)
                query = query.Where(b => b.CheckOutDate <= toDate.Value.Date);

            if (guestId.HasValue)
                query = query.Where(b => b.GuestId == guestId.Value);

            if (hotelId.HasValue)
                query = query.Where(b => b.HotelId == hotelId.Value);

            var bookings = await query
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            return bookings.Select(MapToViewModel).ToList();
        }

        public async Task<BookingViewModel?> GetByIdAsync(long id)
        {
            var booking = await _dbContext.Bookings
                .Include(b => b.Guest)
                .Include(b => b.Hotel)
                .Include(b => b.Promotion)
                .Include(b => b.BookingRooms)
                    .ThenInclude(br => br.Room)
                        .ThenInclude(r => r.RoomType)
                .FirstOrDefaultAsync(b => b.Id == id);

            return booking == null ? null : MapToViewModel(booking);
        }

        public async Task<List<BookingViewModel>> GetByGuestIdAsync(long guestId)
        {
            var bookings = await _dbContext.Bookings
                .Include(b => b.Guest)
                .Include(b => b.Hotel)
                .Include(b => b.Promotion)
                .Include(b => b.BookingRooms)
                    .ThenInclude(br => br.Room)
                        .ThenInclude(r => r.RoomType)
                .Where(b => b.GuestId == guestId)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            return bookings.Select(MapToViewModel).ToList();
        }

        public async Task<(bool Success, string Message, decimal RefundAmount)> CancelBookingAsync(
            long bookingId,
            string cancelReason)
        {
            var booking = await _dbContext.Bookings
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null)
                return (false, "Không tìm thấy booking", 0);

            if (booking.Status == "Cancelled")
                return (false, "Booking đã được hủy trước đó", 0);

            if (booking.Status == "CheckedOut")
                return (false, "Không thể hủy booking đã check-out", 0);

            if (booking.Status == "CheckedIn")
                return (false, "Không thể hủy booking đang check-in. Vui lòng check-out trước.", 0);

            var refundAmount = await CalculateRefundAmountAsync(bookingId);

            booking.Status = "Cancelled";
            booking.CancelledAt = DateTime.UtcNow;

            if (refundAmount > 0 && booking.PaymentStatus == "Paid")
            {
                booking.PaymentStatus = "Refunded";
            }

            await _dbContext.SaveChangesAsync();

            var message = refundAmount > 0
                ? $"Booking đã được hủy thành công. Số tiền hoàn lại: {refundAmount:N0} VNĐ"
                : "Booking đã được hủy thành công. Không hoàn tiền theo chính sách.";

            return (true, message, refundAmount);
        }

        public async Task<bool> CanCancelFreeAsync(long bookingId)
        {
            var booking = await _dbContext.Bookings
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null || booking.Status is "Cancelled" or "CheckedIn" or "CheckedOut")
                return false;

            if (string.IsNullOrEmpty(booking.RatePlanSnapshotJson))
                return false;

            try
            {
                var snapshot = JsonSerializer.Deserialize<JsonElement>(booking.RatePlanSnapshotJson);
                
                if (snapshot.TryGetProperty("Type", out var typeElement) && 
                    typeElement.GetString() == "NonRefundable")
                    return false;

                if (snapshot.TryGetProperty("FreeCancelUntilHours", out var hoursElement) && 
                    hoursElement.TryGetInt32(out var hours))
                {
                    var cancelDeadline = booking.CheckInDate.AddHours(-hours);
                    return DateTime.Now < cancelDeadline;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<decimal> CalculateRefundAmountAsync(long bookingId)
        {
            var booking = await _dbContext.Bookings
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null || booking.Status is "Cancelled" or "CheckedOut")
                return 0;

            var finalAmount = booking.TotalAmount - booking.DiscountAmount;

            if (await CanCancelFreeAsync(bookingId))
                return finalAmount;

            if (!string.IsNullOrEmpty(booking.RatePlanSnapshotJson))
            {
                try
                {
                    var snapshot = JsonSerializer.Deserialize<JsonElement>(booking.RatePlanSnapshotJson);
                    
                    if (snapshot.TryGetProperty("Type", out var typeElement) && 
                        typeElement.GetString() == "NonRefundable")
                        return 0;

                    return finalAmount * 0.5m;
                }
                catch
                {
                    return finalAmount * 0.5m;
                }
            }

            return finalAmount * 0.5m;
        }

        public async Task<(bool Success, string Message)> ModifyBookingAsync(
            long bookingId,
            DateTime? newCheckInDate,
            DateTime? newCheckOutDate,
            long? newRoomId)
        {
            var booking = await _dbContext.Bookings
                .Include(b => b.BookingRooms)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null)
                return (false, "Không tìm thấy booking");

            if (booking.Status is "Cancelled" or "CheckedOut")
                return (false, "Không thể chỉnh sửa booking đã hủy hoặc đã check-out");

            if (booking.Status == "CheckedIn")
                return (false, "Không thể chỉnh sửa booking đang check-in");

            if (newCheckInDate.HasValue && newCheckOutDate.HasValue)
            {
                if (newCheckInDate.Value.Date >= newCheckOutDate.Value.Date)
                    return (false, "Ngày check-in phải trước ngày check-out");

                if (newCheckInDate.Value.Date < DateTime.Today)
                    return (false, "Ngày check-in không thể trong quá khứ");
            }

            var modified = false;

            if (newCheckInDate.HasValue && newCheckOutDate.HasValue)
            {
                var oldRoom = booking.BookingRooms.FirstOrDefault();
                if (oldRoom != null)
                {
                    var hasConflict = await _dbContext.BookingRooms
                        .AnyAsync(br =>
                            br.RoomId == oldRoom.RoomId &&
                            br.BookingId != bookingId &&
                            br.Booking != null &&
                            (br.Booking.Status == "Confirmed" || br.Booking.Status == "CheckedIn") &&
                            br.Booking.CheckInDate < newCheckOutDate.Value &&
                            br.Booking.CheckOutDate > newCheckInDate.Value
                        );

                    if (hasConflict)
                        return (false, "Phòng không khả dụng trong khoảng thời gian mới");

                    booking.CheckInDate = newCheckInDate.Value.Date;
                    booking.CheckOutDate = newCheckOutDate.Value.Date;
                    
                    var nights = (booking.CheckOutDate - booking.CheckInDate).Days;
                    oldRoom.Nights = nights;
                    booking.TotalAmount = oldRoom.PricePerNight * nights;
                    
                    if (booking.PromotionId.HasValue)
                    {
                        var promotion = await _dbContext.Promotions.FindAsync(booking.PromotionId.Value);
                        if (promotion != null)
                        {
                            booking.DiscountAmount = CalculatePromotionDiscount(promotion, booking.TotalAmount);
                        }
                    }

                    modified = true;
                }
            }

            if (newRoomId.HasValue)
            {
                var oldRoom = booking.BookingRooms.FirstOrDefault();
                if (oldRoom != null && oldRoom.RoomId != newRoomId.Value)
                {
                    var newRoom = await _dbContext.Rooms
                        .Include(r => r.RoomType)
                        .FirstOrDefaultAsync(r => r.Id == newRoomId.Value && r.HotelId == booking.HotelId);

                    if (newRoom == null)
                        return (false, "Phòng mới không tồn tại hoặc không thuộc khách sạn này");

                    var hasConflict = await _dbContext.BookingRooms
                        .AnyAsync(br =>
                            br.RoomId == newRoomId.Value &&
                            br.BookingId != bookingId &&
                            br.Booking != null &&
                            (br.Booking.Status == "Confirmed" || br.Booking.Status == "CheckedIn") &&
                            br.Booking.CheckInDate < booking.CheckOutDate &&
                            br.Booking.CheckOutDate > booking.CheckInDate
                        );

                    if (hasConflict)
                        return (false, "Phòng mới không khả dụng trong khoảng thời gian này");

                    oldRoom.RoomId = newRoomId.Value;
                    modified = true;
                }
            }

            if (modified)
            {
                booking.ModifiedAt = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync();
                return (true, "Booking đã được chỉnh sửa thành công");
            }

            return (false, "Không có thay đổi nào được thực hiện");
        }

        public async Task<(bool Success, string Message)> CheckInAsync(long bookingId)
        {
            var booking = await _dbContext.Bookings
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null)
                return (false, "Không tìm thấy booking");

            if (booking.Status != "Confirmed")
                return (false, $"Không thể check-in. Trạng thái hiện tại: {booking.Status}");

            if (booking.CheckInDate.Date > DateTime.Today)
                return (false, "Chưa đến ngày check-in");

            if (booking.CheckInActualDate.HasValue)
                return (false, "Booking đã được check-in trước đó");

            booking.Status = "CheckedIn";
            booking.CheckInActualDate = DateTime.Now;

            await _dbContext.SaveChangesAsync();

            return (true, "Check-in thành công");
        }

        public async Task<(bool Success, string Message)> CheckOutAsync(long bookingId)
        {
            var booking = await _dbContext.Bookings
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null)
                return (false, "Không tìm thấy booking");

            if (booking.Status != "CheckedIn")
                return (false, $"Không thể check-out. Trạng thái hiện tại: {booking.Status}");

            if (booking.CheckOutActualDate.HasValue)
                return (false, "Booking đã được check-out trước đó");

            booking.Status = "CheckedOut";
            booking.CheckOutActualDate = DateTime.Now;

            await _dbContext.SaveChangesAsync();

            return (true, "Check-out thành công");
        }

        public async Task<(bool Success, string Message, decimal DiscountAmount)> ApplyPromotionAsync(
            long bookingId,
            string promotionCode)
        {
            var booking = await _dbContext.Bookings
                .Include(b => b.Guest)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null)
                return (false, "Không tìm thấy booking", 0);

            if (booking.Status is "Cancelled" or "CheckedOut")
                return (false, "Không thể áp dụng khuyến mãi cho booking đã hủy hoặc đã check-out", 0);

            if (booking.PromotionId.HasValue)
                return (false, "Booking đã áp dụng khuyến mãi. Vui lòng xóa mã hiện tại trước khi áp dụng mã mới.", 0);

            var promotion = await _dbContext.Promotions
                .FirstOrDefaultAsync(p => p.Code == promotionCode);

            if (promotion == null)
                return (false, "Mã khuyến mãi không tồn tại", 0);

            var today = DateTime.Today;
            if (today < promotion.StartDate || today > promotion.EndDate)
                return (false, "Mã khuyến mãi không còn hiệu lực", 0);

            if (!string.IsNullOrEmpty(promotion.ConditionsJson))
            {
                try
                {
                    var conditions = JsonSerializer.Deserialize<JsonElement>(promotion.ConditionsJson);

                    if (conditions.TryGetProperty("min_order_value", out var minOrderElement))
                    {
                        var minOrder = minOrderElement.GetDecimal();
                        if (booking.TotalAmount < minOrder)
                            return (false, $"Đơn hàng phải từ {minOrder:N0} VNĐ trở lên", 0);
                    }

                    if (conditions.TryGetProperty("max_usage_count", out var maxUsageElement) &&
                        conditions.TryGetProperty("current_usage_count", out var currentUsageElement))
                    {
                        var maxUsage = maxUsageElement.GetInt32();
                        var currentUsage = currentUsageElement.GetInt32();
                        
                        if (currentUsage >= maxUsage)
                            return (false, "Mã khuyến mãi đã hết lượt sử dụng", 0);
                    }

                    if (conditions.TryGetProperty("is_new_customer_only", out var isNewCustomerElement))
                    {
                        var isNewCustomerOnly = isNewCustomerElement.GetBoolean();
                        if (isNewCustomerOnly)
                        {
                            var bookingCount = await _dbContext.Bookings
                                .CountAsync(b => b.GuestId == booking.GuestId && b.Status != "Cancelled");
                            
                            if (bookingCount > 1)
                                return (false, "Mã khuyến mãi chỉ dành cho khách hàng mới", 0);
                        }
                    }
                }
                catch { }
            }

            var discountAmount = CalculatePromotionDiscount(promotion, booking.TotalAmount);

            booking.PromotionId = promotion.Id;
            booking.DiscountAmount = discountAmount;

            await IncrementPromotionUsageAsync(promotion.Id);
            await _dbContext.SaveChangesAsync();

            return (true, "Áp dụng mã khuyến mãi thành công", discountAmount);
        }

        public async Task<bool> RemovePromotionAsync(long bookingId)
        {
            var booking = await _dbContext.Bookings
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null || !booking.PromotionId.HasValue)
                return false;

            if (booking.PromotionId.HasValue)
            {
                await DecrementPromotionUsageAsync(booking.PromotionId.Value);
            }

            booking.PromotionId = null;
            booking.DiscountAmount = 0;

            await _dbContext.SaveChangesAsync();
            return true;
        }

        private BookingViewModel MapToViewModel(Booking booking)
        {
            var bookingRoom = booking.BookingRooms.FirstOrDefault();
            var room = bookingRoom?.Room;
            var roomType = room?.RoomType;

            string? ratePlanName = null;
            string? ratePlanType = null;
            int? freeCancelHours = null;

            if (!string.IsNullOrEmpty(booking.RatePlanSnapshotJson))
            {
                try
                {
                    var snapshot = JsonSerializer.Deserialize<JsonElement>(booking.RatePlanSnapshotJson);
                    ratePlanName = snapshot.TryGetProperty("Name", out var nameEl) ? nameEl.GetString() : null;
                    ratePlanType = snapshot.TryGetProperty("Type", out var typeEl) ? typeEl.GetString() : null;
                    freeCancelHours = snapshot.TryGetProperty("FreeCancelUntilHours", out var hoursEl) && hoursEl.TryGetInt32(out var hours) ? hours : null;
                }
                catch { }
            }

            return new BookingViewModel
            {
                Id = booking.Id,
                HotelId = booking.HotelId,
                HotelName = booking.Hotel?.Name ?? "",
                GuestId = booking.GuestId,
                GuestName = booking.Guest?.FullName ?? "",
                GuestEmail = booking.Guest?.Email,
                GuestPhone = booking.Guest?.Phone,
                RoomId = room?.Id ?? 0,
                RoomNumber = room?.Number ?? "",
                RoomTypeName = roomType?.Name ?? "",
                CheckInDate = booking.CheckInDate,
                CheckOutDate = booking.CheckOutDate,
                Status = booking.Status,
                PaymentStatus = booking.PaymentStatus,
                TotalAmount = booking.TotalAmount,
                DiscountAmount = booking.DiscountAmount,
                PromotionId = booking.PromotionId,
                PromotionCode = booking.Promotion?.Code,
                RatePlanSnapshotJson = booking.RatePlanSnapshotJson,
                RatePlanName = ratePlanName,
                RatePlanType = ratePlanType,
                FreeCancelUntilHours = freeCancelHours,
                CreatedAt = booking.CreatedAt,
                CancelledAt = booking.CancelledAt,
                ModifiedAt = booking.ModifiedAt,
                CheckInActualDate = booking.CheckInActualDate,
                CheckOutActualDate = booking.CheckOutActualDate
            };
        }

        private decimal CalculatePromotionDiscount(Promotion promotion, decimal orderAmount)
        {
            decimal discount = 0;

            if (promotion.Type == "Percent")
            {
                discount = orderAmount * (promotion.Value / 100);

                if (!string.IsNullOrEmpty(promotion.ConditionsJson))
                {
                    try
                    {
                        var conditions = JsonSerializer.Deserialize<JsonElement>(promotion.ConditionsJson);
                        if (conditions.TryGetProperty("max_discount_amount", out var maxElement))
                        {
                            var maxDiscount = maxElement.GetDecimal();
                            discount = Math.Min(discount, maxDiscount);
                        }
                    }
                    catch { }
                }
            }
            else
            {
                discount = promotion.Value;
            }

            return Math.Min(discount, orderAmount);
        }

        private async Task IncrementPromotionUsageAsync(long promotionId)
        {
            var promotion = await _dbContext.Promotions.FindAsync(promotionId);
            if (promotion != null && !string.IsNullOrEmpty(promotion.ConditionsJson))
            {
                try
                {
                    var conditions = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(promotion.ConditionsJson);
                    if (conditions != null && conditions.ContainsKey("current_usage_count"))
                    {
                        var currentCount = conditions["current_usage_count"].GetInt32();
                        conditions["current_usage_count"] = JsonSerializer.SerializeToElement(currentCount + 1);
                        promotion.ConditionsJson = JsonSerializer.Serialize(conditions);
                        await _dbContext.SaveChangesAsync();
                    }
                }
                catch { }
            }
        }

        private async Task DecrementPromotionUsageAsync(long promotionId)
        {
            var promotion = await _dbContext.Promotions.FindAsync(promotionId);
            if (promotion != null && !string.IsNullOrEmpty(promotion.ConditionsJson))
            {
                try
                {
                    var conditions = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(promotion.ConditionsJson);
                    if (conditions != null && conditions.ContainsKey("current_usage_count"))
                    {
                        var currentCount = conditions["current_usage_count"].GetInt32();
                        if (currentCount > 0)
                        {
                            conditions["current_usage_count"] = JsonSerializer.SerializeToElement(currentCount - 1);
                            promotion.ConditionsJson = JsonSerializer.Serialize(conditions);
                            await _dbContext.SaveChangesAsync();
                        }
                    }
                }
                catch { }
            }
        }
    }
}
