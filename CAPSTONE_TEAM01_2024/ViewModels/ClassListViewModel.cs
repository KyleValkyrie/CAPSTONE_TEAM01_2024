using CAPSTONE_TEAM01_2024.Models;
using CAPSTONE_TEAM01_2024.Utilities;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CAPSTONE_TEAM01_2024.ViewModels
{
    public class ClassListViewModel
    {
        public Class Class { get; set; }    
        public IEnumerable<SelectListItem> Advisors { get; set; }
        public IEnumerable<SelectListItem> Years { get; set; }
        public PaginatedList<Class> PaginatedClasses { get; set; }
    }
}
