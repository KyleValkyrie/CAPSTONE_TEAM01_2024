using System.ComponentModel.DataAnnotations;

namespace CAPSTONE_TEAM01_2024.Models;

public class EmailThread
{
    [Key]
    public int ThreadId { get; set; }

    public string Subject { get; set; }

    public List<Email> Emails { get; set; } = new List<Email>();
}