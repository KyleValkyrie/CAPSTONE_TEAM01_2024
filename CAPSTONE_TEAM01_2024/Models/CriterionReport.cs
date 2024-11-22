using System.ComponentModel.DataAnnotations;
namespace CAPSTONE_TEAM01_2024.Models
{

    public class CriterionReport
    {
        [Key]
        public int CriterionId { get; set; }

        [Required]
        public string NameReport { get; set; }  // Criterion title

        public string DescriptionReport { get; set; }  // Description for the criterion

        // More fields could be added here as needed for criteria specifics

    }
}