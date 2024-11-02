using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CAPSTONE_TEAM01_2024.Models
{
    public class PlanDetail
    {
        [Key]
        public int DetailId { get; set; }

        [Required]
        public int PlanId { get; set; }
        [JsonIgnore]
        public SemesterPlan SemesterPlan { get; set; }

        [Required]
        public int CriterionId { get; set; }
        public Criterion Criterion { get; set; }
        public string Task { get; set; } // Task for each criterion
        public string ?HowToExecute { get; set; }  // Steps for execution
        public string ?Quantity { get; set; }  // Quantity or extent
        public string ?TimeFrame { get; set; }  // Timeframe for completion
        public string ?Notes { get; set; }  // Additional notes
    }
}
