using System.ComponentModel.DataAnnotations;

namespace CAPSTONE_TEAM01_2024.Models
{
    public class Criterion
    {
        [Key]
        public int CriterionId { get; set; }

        [Required]
        public string Name { get; set; }  // Criterion title

        public string Description { get; set; }  // Description for the criterion

        // More fields could be added here as needed for criteria specifics
    }
}
