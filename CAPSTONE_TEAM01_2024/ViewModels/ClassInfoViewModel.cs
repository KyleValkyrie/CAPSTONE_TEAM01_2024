using CAPSTONE_TEAM01_2024.Models;

namespace CAPSTONE_TEAM01_2024.ViewModels
{
    public class ClassInfoViewModel
    {
        public Class Class { get; set; }
        public List<ClassInfo> ClassInfos { get; set; }
        public AcademicPeriod AcademicPeriod { get; set; }
    }
}
