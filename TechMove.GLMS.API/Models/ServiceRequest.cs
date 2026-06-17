using TechMove.GLMS.API.Models.Enums;

namespace TechMove.GLMS.API.Models
{
    public class ServiceRequest
    {
        public int Id { get; set; }

        public int ContractId { get; set; }
        public Contract? Contract { get; set; }

        public string Description { get; set; } = string.Empty;

        // ENUM STATUS (correct)
        public ServiceRequestStatus Status { get; set; } = ServiceRequestStatus.Pending;

        public decimal CostUSD { get; set; }
        public decimal CostZAR { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // ⭐ ADD THIS — your views depend on it
        public DateTime? CompletedAt { get; set; }
    }
}
