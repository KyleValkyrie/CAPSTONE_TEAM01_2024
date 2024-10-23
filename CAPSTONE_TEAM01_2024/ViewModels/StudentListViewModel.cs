namespace CAPSTONE_TEAM01_2024.ViewModels
{
    public class StudentListViewModel
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string SchoolId { get; set; }
        public string FullName { get; set; }
        public DateTime? DateOfBirth { get; set; } // New field
        public string Status { get; set; } // New field
        public string ClassId { get; set; } // New field
    }
}
