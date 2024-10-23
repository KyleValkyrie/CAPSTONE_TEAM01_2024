using System.ComponentModel.DataAnnotations;

namespace CAPSTONE_TEAM01_2024.Models
{
    public class Announcement
    {
		[Key]
		public int Id { get; set; }
		[Required]
		public string Detail { get; set; }
		[Required]
		public DateTime StartTime { get; set; }
		[Required]
		public DateTime EndTime { get; set; }
		[Required]
		public string Type { get; set; }
    }
}
