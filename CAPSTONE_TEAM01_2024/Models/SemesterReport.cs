using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CAPSTONE_TEAM01_2024.Models
{


    public class SemesterReport
    {
        [Key] 
        public int ReportId { get; set; }

        [Required] 
        public string PeriodName { get; set; } // Name of the academic period

        [Required] 
        public string ReportType { get; set; } // Type of plan

        [Required] 
        public string ClassId { get; set; } // Foreign Key to Class
        public Class Class { get; set; }

        [Required]
        public string AdvisorName { get; set; } // Name of the advisor

        [Required] 
        public DateTime CreationTimeReport { get; set; } // Creation time of the report

        [Required] 
        public string StatusReport { get; set; } // Status of the plan ("Đã Nộp", "Chưa Nộp", "Nháp")

        // Foreign Key and Navigation Property for AcademicPeriod
        [Required] 
        public int PeriodId { get; set; }
        [JsonIgnore] 
        public AcademicPeriod AcademicPeriod { get; set; }

        // Navigation property for ReportDetails
        public ICollection<ReportDetail> ReportDetails { get; set; } = new List<ReportDetail>();

        // Cá Nhân Tự Đánh Giá và Xếp Loại
        public string? SelfAssessment { get; set; } // Cá nhân tự đánh giá
        public char? SelfRanking { get; set; } // Cá nhân tự xếp loại


        // BCN Khoa Đánh Giá và Xếp Loại
        public string? FacultyAssessment { get; set; } // BCN Khoa đánh giá
        public char? FacultyRanking { get; set; } // BCN Khoa xếp loại
    }
}