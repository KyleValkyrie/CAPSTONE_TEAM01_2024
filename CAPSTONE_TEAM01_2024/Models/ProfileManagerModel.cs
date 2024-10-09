using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CAPSTONE_TEAM01_2024.Models
{
    public class ProfileManagerModel
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Email không được để trống")]
        public string UserId { get; set; }
        public string? Email { get; set; }
        [ForeignKey("UserId")]
        public IdentityUser User { get; set; }
        
        [Required(ErrorMessage = "Mã Số không được để trống")]
        public string? MaSo { get; set; }

        [Required(ErrorMessage = "Tên Đầy ĐỦ không được để trống")]
        public string? TenDayDu { get; set; }

        
        [Required(ErrorMessage = "Số Điện Thoại không được để trống")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone number must be exactly 10 digits.")]
        public int SoDienThoai { get; set; }

        [Required(ErrorMessage = "Vai Trò không được để trống")]
        public string? VaiTro { get; set; }
    
    }
}
