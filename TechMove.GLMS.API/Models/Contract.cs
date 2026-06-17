using TechMove.GLMS.API.Models.Enums;

namespace TechMove.GLMS.API.Models
{
    public class Contract
    {
        public int Id { get; set; }

        public string ContractNumber { get; set; } = string.Empty;

        public string ClientName { get; set; } = string.Empty;

        public string ServiceLevel { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        // ENUM STATUS (correct)
        public ContractStatus Status { get; set; } = ContractStatus.Pending;

        // ⭐ ADD THESE BACK — MVC depends on them
        public decimal BaseRate { get; set; }

        public decimal PenaltyRate { get; set; }

        public string? SignedAgreementPath { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();

        public int ClientId { get; set; }
    }
}
