using System.ComponentModel.DataAnnotations;
using TechMove.GLMS.API.Models;

namespace TechMove.GLMS.ViewModels
{
    public class ServiceRequestCreateViewModel
    {
        // Form Fields
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a contract.")]
        public int ContractId { get; set; }

        [Required]
        [StringLength(200)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public decimal CostUSD { get; set; }

        // Dropdown Data
        public List<Contract> Contracts { get; set; } = new();
    }
}
