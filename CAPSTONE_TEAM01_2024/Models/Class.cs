using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CAPSTONE_TEAM01_2024.Models
{
    public class Class
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string ClassName { get; set; }

        [Required]
        public int AdvisorId { get; set; }

		[Required]
		[StringLength(100)]
		public string AdvisorName { get; set; }

		[ForeignKey("AdvisorId")]
        public ProfileManagerModel Advisor { get; set; }

		[Required]
		[StringLength(20)]
		public string YearName { get; set; }

        [Required]
        [StringLength(100)]
        public string Department { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int StudentCount { get; set; }

		public ICollection<Student> Students { get; set; } // Collection of Students
	}
}
