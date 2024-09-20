using System.ComponentModel.DataAnnotations;

namespace CAPSTONE_TEAM01_2024.Models
{
    public class AcademicPeriod
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Range(1900, 2100, ErrorMessage = "Please enter a valid year")]
        public int Year { get; set; }

        [Required]
        [Range(1, 3, ErrorMessage = "Semester must be between 1 and 3")]
        public int Semester { get; set; }

        // Navigation property
        public ICollection<Class> Classes { get; set; }
    }
}
