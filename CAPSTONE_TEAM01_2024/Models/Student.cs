using System.ComponentModel.DataAnnotations;

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
		[StringLength(10)]
		public string ClassCode { get; set; }

		[Required]
		[StringLength(50)]
		public string Status { get; set; }
	}
}
