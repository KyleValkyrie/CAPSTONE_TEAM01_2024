using CAPSTONE_TEAM01_2024.Models;
using CAPSTONE_TEAM01_2024.Utilities;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CAPSTONE_TEAM01_2024.ViewModels
{
	public class ProfileManagerViewModel
	{	
		public ProfileManagerModel ProfileManagerModel { get; set; }
		public string SelectedEmail { get; set; }
		public List<SelectListItem> UserEmail {  get; set; }
		
		public PaginatedList<ProfileManagerModel> PaginatedProfile { get; set; }
	}
}
