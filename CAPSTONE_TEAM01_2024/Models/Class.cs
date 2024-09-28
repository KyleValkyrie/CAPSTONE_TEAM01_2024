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
        public int YearId { get; set; }

		[Required]
		[StringLength(100)]
		public string YearName { get; set; }

		[ForeignKey("YearId")]
        public AcademicPeriod Year { get; set; }

        [Required]
        [StringLength(100)]
        public string Department { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int StudentCount { get; set; }
    }
}
