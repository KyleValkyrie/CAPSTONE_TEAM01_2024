using System.ComponentModel.DataAnnotations;

namespace CAPSTONE_TEAM01_2024.Models
{
    public class Class
    {
        [Key]
        public int ClassId { get; set; }

        [Required]
        public string ClassName { get; set; }

        [Required]
        public string Advisor { get; set; }

        [Required]
        public int StudentCount { get; set; }

        [Required]
        public int EnrollmentNumber { get; set; }

        [Required]
        public int AcademicPeriodId { get; set; }

        // Navigation property
        public AcademicPeriod AcademicPeriod { get; set; }

        // Navigation property for students
        public ICollection<ClassInfo> ClassInfos { get; set; }
    }
}
