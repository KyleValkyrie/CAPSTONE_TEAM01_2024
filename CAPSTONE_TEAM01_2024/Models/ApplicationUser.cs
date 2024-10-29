using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CAPSTONE_TEAM01_2024.Models
{
	public class ApplicationUser : IdentityUser
    {
        // Additional properties
        public string SchoolId { get; set; }
        public bool IsRegistered { get; set; } = false;
        public string? FullName { get; set; }
       
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
        public override string PhoneNumber { get; set; }


        public DateTime? LastLoginTime { get; set; }

        // Navigation properties
        [JsonIgnore]
        public ICollection<Class> AdvisedClasses { get; set; } = new List<Class>(); // Classes they advise
        public string ClassId { get; set; } // Foreign Key to Class
        [JsonIgnore]
        public Class EnrolledClass { get; set; } // The class the student is enrolled in
        public DateTime DateOfBirth { get; set; } // Addition field for students
        public string Status { get; set; } // Addition field for students
    }
}
