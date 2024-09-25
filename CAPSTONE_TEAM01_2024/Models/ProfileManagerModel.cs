﻿using System.ComponentModel.DataAnnotations;

namespace CAPSTONE_TEAM01_2024.Models
{
    public class ProfileManagerModel
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Email không được để trống")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Mã Số không được để trống")]
        public string? MaSo { get; set; }

        [Required(ErrorMessage = "Tên Đầy ĐỦ không được để trống")]
        public string? TenDayDu { get; set; }

        [Required(ErrorMessage = "Số Điện Thoại không được để trống")]

        public int SoDienThoai { get; set; }

        [Required(ErrorMessage = "Vai Trò không được để trống")]
        public string? VaiTro { get; set; }
    
    }
}
