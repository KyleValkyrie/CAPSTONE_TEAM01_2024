using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace CAPSTONE_TEAM01_2024.Models
{
	public class UserAnnouncement
	{
		[Key]
		public int Id { get; set; }

		[Required]
		public string UserId { get; set; }

		[Required]
		public int AnnouncementId { get; set; }

		[Required]
		public bool IsRead { get; set; }

		[Required]
		public DateTime CreatedAt { get; set; }

		// Navigation properties
		public ApplicationUser User { get; set; }
		public Announcement Announcement { get; set; }
	}
}
