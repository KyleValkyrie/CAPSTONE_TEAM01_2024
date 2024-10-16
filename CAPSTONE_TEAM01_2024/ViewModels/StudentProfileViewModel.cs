namespace CAPSTONE_TEAM01_2024.ViewModels
{
    public class StudentProfileViewModel
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string SchoolId { get; set; }
        public string FullName { get; set; }
        public bool IsRegistered { get; set; }
        public DateTime? LastLoginTime { get; set; }
        public string Role{get; set;} //Role Property
    }
}
