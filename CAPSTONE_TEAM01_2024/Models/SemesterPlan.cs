using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CAPSTONE_TEAM01_2024.Models
{
    public class SemesterPlan
    {
        [Key]
        public int PlanId { get; set; }

        [Required]
        public string PeriodName { get; set; }  // Name of the academic period

        [Required]
        public string PlanType { get; set; }  // Type of plan

        [Required]
        public string ClassId { get; set; }  // Foreign Key to Class
        public Class Class { get; set; }

        [Required]
        public string AdvisorName { get; set; }  // Name of the advisor

        [Required]
        public DateTime CreationTime { get; set; }  // Creation time of the plan

        public byte[]? ProofFile { get; set; }  // File data directly in SemesterPlan

        public string? ProofFileName { get; set; }  // Store file name for referenc

        [Required]
        public string Status { get; set; }  // Status of the plan ("Đã Nộp", "Chưa Nộp", "Nháp")

        // Foreign Key and Navigation Property for AcademicPeriod
        [Required]
        public int PeriodId { get; set; }
        [JsonIgnore]
        public AcademicPeriod AcademicPeriod { get; set; }
    }
}
