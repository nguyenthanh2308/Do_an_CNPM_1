using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace HotelManagementSystem.Models.ViewModels.Promotion
{
    public class PromotionViewModel
    {
        public long Id { get; set; }

        [Required(ErrorMessage = "Mã khuyến mãi không được để trống")]
        [StringLength(32, ErrorMessage = "Mã khuyến mãi không được vượt quá 32 ký tự")]
        [RegularExpression(@"^[A-Z0-9_-]+$", ErrorMessage = "Mã chỉ được chứa chữ in hoa, số, gạch ngang và gạch dưới")]
        [Display(Name = "Mã khuyến mãi")]
        public string Code { get; set; } = null!;

        [Required(ErrorMessage = "Loại khuyến mãi không được để trống")]
        [Display(Name = "Loại khuyến mãi")]
        public string Type { get; set; } = "Percent";

        [Required(ErrorMessage = "Giá trị không được để trống")]
        [Range(0.01, 999999999.99, ErrorMessage = "Giá trị phải lớn hơn 0")]
        [Display(Name = "Giá trị")]
        public decimal Value { get; set; }

        [Required(ErrorMessage = "Ngày bắt đầu không được để trống")]
        [Display(Name = "Ngày bắt đầu")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Ngày kết thúc không được để trống")]
        [Display(Name = "Ngày kết thúc")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        // Conditions fields
        [Display(Name = "Giá trị đơn hàng tối thiểu")]
        [Range(0, 999999999.99, ErrorMessage = "Giá trị phải từ 0 trở lên")]
        public decimal? MinOrderValue { get; set; }

        [Display(Name = "Giảm giá tối đa")]
        [Range(0, 999999999.99, ErrorMessage = "Giá trị phải từ 0 trở lên")]
        public decimal? MaxDiscountAmount { get; set; }

        [Display(Name = "Số lần sử dụng tối đa")]
        [Range(1, 999999, ErrorMessage = "Số lần sử dụng phải từ 1 trở lên")]
        public int? MaxUsageCount { get; set; }

        [Display(Name = "Số lần đã sử dụng")]
        public int CurrentUsageCount { get; set; }

        [Display(Name = "Chỉ áp dụng cho khách hàng mới")]
        public bool IsNewCustomerOnly { get; set; }

        public DateTime CreatedAt { get; set; }

        // Helper properties
        public string TypeDisplay => Type switch
        {
            "Percent" => "Phần trăm (%)",
            "Amount" => "Số tiền cố định",
            _ => Type
        };

        public string ValueDisplay
        {
            get
            {
                if (Type == "Percent")
                    return $"{Value}%";
                else
                    return $"{Value:N0} VNĐ";
            }
        }

        public bool IsCurrentlyActive
        {
            get
            {
                var now = DateTime.Now.Date;
                return now >= StartDate.Date && now <= EndDate.Date;
            }
        }

        public bool IsExpired => DateTime.Now.Date > EndDate.Date;

        public bool IsUpcoming => DateTime.Now.Date < StartDate.Date;

        public int DaysRemaining
        {
            get
            {
                if (IsExpired) return 0;
                return (EndDate.Date - DateTime.Now.Date).Days;
            }
        }

        public bool IsUsageLimitReached
        {
            get
            {
                if (!MaxUsageCount.HasValue) return false;
                return CurrentUsageCount >= MaxUsageCount.Value;
            }
        }

        public string StatusDisplay
        {
            get
            {
                if (IsUsageLimitReached) return "Đã hết lượt sử dụng";
                if (IsExpired) return "Đã hết hạn";
                if (IsUpcoming) return "Sắp áp dụng";
                if (IsCurrentlyActive) return "Đang hoạt động";
                return "Không xác định";
            }
        }

        public string StatusBadgeClass
        {
            get
            {
                if (IsUsageLimitReached) return "bg-dark";
                if (IsExpired) return "bg-secondary";
                if (IsUpcoming) return "bg-info";
                if (IsCurrentlyActive) return "bg-success";
                return "bg-secondary";
            }
        }

        // Calculate discount amount
        public decimal CalculateDiscount(decimal orderAmount)
        {
            if (Type == "Percent")
            {
                var discount = orderAmount * (Value / 100);
                if (MaxDiscountAmount.HasValue && discount > MaxDiscountAmount.Value)
                {
                    return MaxDiscountAmount.Value;
                }
                return discount;
            }
            else // Amount
            {
                return Value;
            }
        }

        // Validate if promotion can be applied
        public bool CanApplyToOrder(decimal orderAmount, bool isNewCustomer)
        {
            // Check if active
            if (!IsCurrentlyActive) return false;

            // Check usage limit
            if (IsUsageLimitReached) return false;

            // Check minimum order value
            if (MinOrderValue.HasValue && orderAmount < MinOrderValue.Value) return false;

            // Check new customer requirement
            if (IsNewCustomerOnly && !isNewCustomer) return false;

            return true;
        }

        // Build conditions JSON
        public string BuildConditionsJson()
        {
            var conditions = new
            {
                min_order_value = MinOrderValue,
                max_discount_amount = MaxDiscountAmount,
                max_usage_count = MaxUsageCount,
                current_usage_count = CurrentUsageCount,
                is_new_customer_only = IsNewCustomerOnly
            };

            return JsonSerializer.Serialize(conditions);
        }

        // Parse conditions JSON
        public void ParseConditionsJson(string? json)
        {
            if (string.IsNullOrWhiteSpace(json)) return;

            try
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (root.TryGetProperty("min_order_value", out var minOrder) && minOrder.ValueKind != JsonValueKind.Null)
                    MinOrderValue = minOrder.GetDecimal();

                if (root.TryGetProperty("max_discount_amount", out var maxDiscount) && maxDiscount.ValueKind != JsonValueKind.Null)
                    MaxDiscountAmount = maxDiscount.GetDecimal();

                if (root.TryGetProperty("max_usage_count", out var maxUsage) && maxUsage.ValueKind != JsonValueKind.Null)
                    MaxUsageCount = maxUsage.GetInt32();

                if (root.TryGetProperty("current_usage_count", out var currentUsage) && currentUsage.ValueKind != JsonValueKind.Null)
                    CurrentUsageCount = currentUsage.GetInt32();

                if (root.TryGetProperty("is_new_customer_only", out var newCustomerOnly) && newCustomerOnly.ValueKind != JsonValueKind.Null)
                    IsNewCustomerOnly = newCustomerOnly.GetBoolean();
            }
            catch
            {
                // Ignore parsing errors
            }
        }
    }
}
