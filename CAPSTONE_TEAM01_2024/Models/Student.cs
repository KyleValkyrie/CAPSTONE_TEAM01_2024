using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CAPSTONE_TEAM01_2024.Models
{
	public class Student
	{
		[Key]
		[Required]
		[StringLength(10)]
		public string Id { get; set; }

		[Required]
		[EmailAddress]
		public string Email { get; set; }

		[Required]
		[StringLength(100)]
		public string FullName { get; set; }

		[Required]
		public DateTime DateOfBirth { get; set; }

		[Required]
		public int ClassId { get; set; }

		// Foreign Key to Class
		[ForeignKey("ClassId")]
		public Class Class { get; set; }

		[Required]
		public string Status { get; set; }

		// Foreign Key to ProfileManager
		public int ProfileManagerId { get; set; }
		[ForeignKey("ProfileManagerId")]
		public ProfileManagerModel ProfileManager { get; set; }
	}
}
