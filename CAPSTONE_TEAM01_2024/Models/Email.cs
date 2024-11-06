using System.ComponentModel.DataAnnotations;
namespace CAPSTONE_TEAM01_2024.Models;

public class Email
{
    [Key]
    public int EmailId { get; set; }

    [Required]
    public string Subject { get; set; }

    [Required]
    public string Content { get; set; }
    [Required]
    public DateTime SentDate { get; set; } = DateTime.Now;
    [Required]
    public string Status { get; set; } = "Draft"; // Options: Sent, Draft, Read, Unread

    // Use ApplicationUser for sender
    [Required]
    public string SenderId { get; set; }
    public ApplicationUser Sender { get; set; }

    public int? ThreadId { get; set; }
    public EmailThread Thread { get; set; }

    public List<EmailRecipient> Recipients { get; set; } = new List<EmailRecipient>();
    public List<EmailAttachment> Attachments { get; set; } = new List<EmailAttachment>();
}
