using System.ComponentModel.DataAnnotations;

namespace CAPSTONE_TEAM01_2024.Models
{
    public class ClassInfo
    {
        [Key]
        public string StudentId { get; set; }

        [Required]
        public string StudentName { get; set; }

        // Foreign key to Class
        public int ClassId { get; set; }

        // Navigation property
        public Class Class { get; set; }
    }
}
