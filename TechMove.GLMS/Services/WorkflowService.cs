using TechMove.GLMS.API.Models;
using TechMove.GLMS.API.Models.Enums;

namespace TechMove.GLMS.Services
{
    public class WorkflowService
    {
        // Automatically update contract status based on dates
        public void UpdateContractStatus(Contract contract)
        {
            if (contract.EndDate < DateTime.Today)
                contract.Status = ContractStatus.Expired;
            else
                contract.Status = ContractStatus.Active;
        }

        // Prevent service requests on expired contracts
        public bool CanCreateServiceRequest(Contract contract)
        {
            return contract.Status == ContractStatus.Active;
        }
    }
}
