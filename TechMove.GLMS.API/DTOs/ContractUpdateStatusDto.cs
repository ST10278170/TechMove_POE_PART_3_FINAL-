using TechMove.GLMS.API.Models.Enums;

namespace TechMove.GLMS.API.DTOs
{
    public class ContractUpdateStatusDto
    {
        // FIXED: Status must be an ENUM, not a string
        public ContractStatus Status { get; set; }
    }
}
