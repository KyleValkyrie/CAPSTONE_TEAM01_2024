using CAPSTONE_TEAM01_2024.Models;
using CAPSTONE_TEAM01_2024.Utilities;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CAPSTONE_TEAM01_2024.ViewModels
{
    public class SemesterPlanViewModel
    {
        public PaginatedList<SemesterPlan> SemesterPlans { get; set; }
        public IEnumerable<SelectListItem> SchoolYears { get; set; }
        public IEnumerable<SelectListItem> Class { get; set; }
    }
}
