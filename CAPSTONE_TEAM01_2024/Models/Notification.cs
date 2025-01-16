using System.ComponentModel.DataAnnotations;

namespace CAPSTONE_TEAM01_2024.Models
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Message { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; }
        [Required]
        public string TargetedUserEmail { get; set; }  // UserIds of the recipients
        [Required]
        public bool IsRead { get; set; }
    }
}
