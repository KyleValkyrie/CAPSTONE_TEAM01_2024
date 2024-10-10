using CAPSTONE_TEAM01_2024.Models;
using CAPSTONE_TEAM01_2024.Utilities;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CAPSTONE_TEAM01_2024.ViewModels
{
	public class StudentListViewModel
	{
		public Student Student { get; set; }
		public Class Class { get; set; }
		public IEnumerable<SelectListItem> StudentList { get; set; }
		public PaginatedList<Student> PaginatedStudents { get; set; }
		public IEnumerable<ProfileManagerModel> ProfileManagers { get; set; }
	}

}
