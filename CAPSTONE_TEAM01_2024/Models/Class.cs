using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CAPSTONE_TEAM01_2024.Models
{
    public class Class
    {
        [Key]
        public string ClassId { get; set; } // Unique identifier, e.g., 71K27CNTT01
        [Required]
        public string Term { get; set; } // Nien Khoa, e.g., 2022-2026
        [Required]
        public string Department { get; set; } // Nganh, e.g., Information Technology
        public string? AdvisorId { get; set; } // Foreign Key to ApplicationUser
        [JsonIgnore]
        public ApplicationUser Advisor { get; set; }
        [Required]
        public int StudentCount { get; set; } // Maximum number of students
        public ICollection<ApplicationUser> Students { get; set; } = new List<ApplicationUser>(); // Collection of students in the class
        [JsonIgnore]
        public ICollection<SemesterPlan> SemesterPlans { get; set; }
        public ICollection<SemesterReport> SemesterReports { get; set; }
    }

}
