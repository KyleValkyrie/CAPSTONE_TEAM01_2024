using System.ComponentModel.DataAnnotations;

namespace CAPSTONE_TEAM01_2024.Models;

public class EmailRecipient
{
    [Key]
    public int RecipientId { get; set; }

    [Required]
    public int EmailId { get; set; }
    public Email Email { get; set; }

    [Required]
    public string UserId { get; set; } // Foreign Key to ApplicationUser
    public ApplicationUser User { get; set; }

    [Required]
    public string RecipientType { get; set; } // Options: To, Cc, Bcc
}

