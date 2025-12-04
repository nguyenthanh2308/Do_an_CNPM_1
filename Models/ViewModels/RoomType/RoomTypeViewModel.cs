using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using HotelManagementSystem.Models.ViewModels.Amenity;

namespace HotelManagementSystem.Models.ViewModels.RoomType
{
    public class RoomTypeViewModel
    {
        public long Id { get; set; }

        [Required(ErrorMessage = "Khách sạn không được để trống")]
        [Display(Name = "Khách sạn")]
        public long HotelId { get; set; }

        [Required(ErrorMessage = "Tên loại phòng không được để trống")]
        [StringLength(128, ErrorMessage = "Tên loại phòng không được vượt quá 128 ký tự")]
        [Display(Name = "Tên loại phòng")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Sức chứa không được để trống")]
        [Range(1, 20, ErrorMessage = "Sức chứa phải từ 1 đến 20 người")]
        [Display(Name = "Sức chứa (người)")]
        public byte Capacity { get; set; }

        [Required(ErrorMessage = "Giá cơ bản không được để trống")]
        [Range(0, 999999999.99, ErrorMessage = "Giá phải từ 0 đến 999,999,999.99")]
        [Display(Name = "Giá cơ bản (VNĐ)")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal BasePrice { get; set; }

        [Display(Name = "Mô tả")]
        [DataType(DataType.MultilineText)]
        public string? Description { get; set; }

        [Display(Name = "URL hình ảnh")]
        [StringLength(255, ErrorMessage = "URL không được vượt quá 255 ký tự")]
        public string? DefaultImageUrl { get; set; }

        [Display(Name = "Upload hình ảnh")]
        public IFormFile? ImageFile { get; set; }

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        [Display(Name = "Tên khách sạn")]
        public string? HotelName { get; set; }

        // Amenities
        [Display(Name = "Tiện nghi")]
        public List<long> SelectedAmenityIds { get; set; } = new List<long>();

        public List<AmenityViewModel> Amenities { get; set; } = new List<AmenityViewModel>();
        
        public List<string> AmenityNames { get; set; } = new List<string>();

        // For dropdown selections
        public Microsoft.AspNetCore.Mvc.Rendering.SelectList? Hotels { get; set; }

        // Statistics
        [Display(Name = "Số phòng")]
        public int RoomCount { get; set; }

        [Display(Name = "Số rate plan")]
        public int RatePlanCount { get; set; }
    }

    public class AmenityCheckboxViewModel
    {
        public long Id { get; set; }
        public string Name { get; set; } = null!;
        public bool IsSelected { get; set; }
    }
}
