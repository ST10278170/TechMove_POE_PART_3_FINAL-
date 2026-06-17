using TechMove.GLMS.API.Models;

namespace TechMove.GLMS.ViewModels
{
    public class DashboardViewModel
    {
        // KPI Tiles
        public int TotalClients { get; set; }
        public int TotalContracts { get; set; }
        public int ActiveContracts { get; set; }
        public int ExpiredContracts { get; set; }
        public int OnHoldContracts { get; set; }

        public int TotalServiceRequests { get; set; }
        public int PendingRequests { get; set; }
        public int CompletedRequests { get; set; }

        // Charts
        public List<string> Last6MonthsLabels { get; set; } = new();
        public List<int> ContractsPerMonth { get; set; } = new();
        public List<int> RequestsPerMonth { get; set; } = new();

        // Recent Activity
        public List<ServiceRequest> RecentRequests { get; set; } = new();

        // Expiring Contracts
        public List<Contract> ExpiringContracts { get; set; } = new();
    }
}
