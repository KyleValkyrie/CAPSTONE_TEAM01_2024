using System.ComponentModel.DataAnnotations;

namespace CAPSTONE_TEAM01_2024.Models;

public class EmailAttachment
{
    [Key]
    public int AttachmentId { get; set; }

    [Required]
    public string FileName { get; set; }

    [Required]
    public byte[] FileData { get; set; }

    [Required]
    public int EmailId { get; set; }
    public Email Email { get; set; }
}