using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CAPSTONE_TEAM01_2024.Models
{
	public class AcademicPeriod
	{
		[Key]
		public int PeriodId { get; set; }
		[Required]
		public string PeriodName { get; set; }
		[Required]
		public DateTime PeriodStart { get; set; }
		[Required]
		public DateTime PeriodEnd { get; set; }
        [JsonIgnore]
        public ICollection<SemesterPlan> SemesterPlans { get; set; }
    }
}
