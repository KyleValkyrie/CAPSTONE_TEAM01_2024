using System.ComponentModel.DataAnnotations;
namespace CAPSTONE_TEAM01_2024.Models;

public class AttachmentReport
{
    [Key]
    public int AttachmentReportId { get; set; }

    [Required]
    public string FileNames { get; set; }
    [Required]
    public string FilePath { get; set; }
    [Required]
    public byte[] FileDatas { get; set; }
    [Required]
    public int DetailReportlId { get; set; }
    public  ReportDetail DetailReport { get; set; }

    
}