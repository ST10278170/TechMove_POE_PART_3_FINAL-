using TechMove.GLMS.API.Models.Enums;

namespace TechMove.GLMS.API.DTOs
{
    public class ServiceRequestUpdateDto
    {
        public string Description { get; set; } = string.Empty;

        // FIXED: Status must be an ENUM, not a string
        public ServiceRequestStatus Status { get; set; }

        public decimal CostUSD { get; set; }
        public decimal CostZAR { get; set; }
    }
}
