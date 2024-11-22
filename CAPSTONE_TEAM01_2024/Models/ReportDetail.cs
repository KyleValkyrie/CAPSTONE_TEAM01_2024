using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CAPSTONE_TEAM01_2024.Models
{

    public class ReportDetail
    {
        [Key] 
        public int DetailReportlId { get; set; }

        [Required] 
        public int ReportId { get; set; }
        [JsonIgnore] 
        public SemesterReport SemesterReport { get; set; }

        [Required] 
        public int CriterionId { get; set; }
        public CriterionReport CriterionReport { get; set; }
        
        public string TaskReport { get; set; } // Task for each criterion
        public string? HowToExecuteReport { get; set; } // Steps for execution
        public List<AttachmentReport> AttachmentReport { get; set; } = new List<AttachmentReport>();
        // Cá Nhân Tự Đánh Giá và Xếp Loại
        public string SelfAssessment { get; set; } // Cá nhân tự đánh giá
        public Ranking SelfRanking { get; set; } // Cá nhân tự xếp loại
        
        // Khai báo Enum trong Model
        public enum Ranking
        {
            A,
            B,
            C,
            D,
            E
        }

        // BCN Khoa Đánh Giá và Xếp Loại
        public string FacultyAssessment { get; set; } // BCN Khoa đánh giá
        public Ranking FacultyRanking { get; set; } // BCN Khoa xếp loại
    
    }
}