using TechMove.GLMS.Helpers;
using TechMove.GLMS.API.Models;

namespace TechMove.GLMS.ViewModels
{
    public class ContractSearchViewModel
    {
        // Search Filters
        public string? SearchTerm { get; set; }
        public string? Status { get; set; }

        // Pagination
        public int PageNumber { get; set; } = 1;

        // Results
        public PaginatedList<Contract>? Results { get; set; }
    }
}
