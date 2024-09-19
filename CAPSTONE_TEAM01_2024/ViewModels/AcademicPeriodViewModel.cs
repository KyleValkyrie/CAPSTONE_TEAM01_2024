using CAPSTONE_TEAM01_2024.Models;

namespace CAPSTONE_TEAM01_2024.ViewModels
{
    public class AcademicPeriodViewModel
    {
        public List<AcademicPeriod> AcademicPeriods { get; set; }
        public int PageNumber { get; set; }
        public int TotalPages { get; set; }
    }
}
