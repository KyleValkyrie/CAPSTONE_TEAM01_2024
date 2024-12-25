using CAPSTONE_TEAM01_2024.Models;

namespace CAPSTONE_TEAM01_2024.ViewModels
{
    public class ShowStatisticViewModel
    {
        public int FacultyNumber { get; set; }
        public int AdvisorNumber { get; set; }
        public int StudentNumber { get; set; }
        public int ClassNumber { get; set; }
        public List<AcademicPeriod> Years { get; set; }
        public List<int> PlanChart { get; set;}
        public List<int> ReportChart { get; set; }
    }
    
}
