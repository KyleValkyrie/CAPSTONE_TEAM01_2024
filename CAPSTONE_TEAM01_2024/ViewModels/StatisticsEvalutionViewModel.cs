using CAPSTONE_TEAM01_2024.Models;
using CAPSTONE_TEAM01_2024.Utilities;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CAPSTONE_TEAM01_2024.ViewModels
{
    public class StatisticsEvalutionViewModel
    {
        public PaginatedList<Class> Classes { get; set; }
        public PaginatedList<SemesterReport> SemesterReports { get; set; }
        
        public IEnumerable<SelectListItem> SchoolYears { get; set; }
        
        public IEnumerable<SelectListItem> Class { get; set; }
        
        public IEnumerable<ReportDetail> ReportDetails { get; set; }
    }
}

