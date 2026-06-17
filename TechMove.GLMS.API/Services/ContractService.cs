using TechMove.GLMS.API.DTOs;
using TechMove.GLMS.API.Models;
using TechMove.GLMS.API.Models.Enums;
using TechMove.GLMS.API.Repositories;

namespace TechMove.GLMS.API.Services
{
    public class ContractService : IContractService
    {
        private readonly IContractRepository _repo;

        public ContractService(IContractRepository repo)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<Contract>> GetAllAsync(string? status = null)
        {
            return await _repo.GetAllAsync(status);
        }

        public async Task<Contract?> GetByIdAsync(int id)
        {
            return await _repo.GetByIdAsync(id);
        }

        public async Task<Contract> CreateAsync(ContractCreateDto dto)
        {
            var contract = new Contract
            {
                ClientName = dto.ClientName,
                ServiceLevel = dto.ServiceLevel,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,

                // FIXED: Status must be enum
                Status = ContractStatus.Pending,

                ContractNumber = $"CNT-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}"
            };

            await _repo.AddAsync(contract);
            await _repo.SaveChangesAsync();

            return contract;
        }

        public async Task<bool> UpdateStatusAsync(int id, ContractStatus newStatus)
        {
            var contract = await _repo.GetByIdAsync(id);
            if (contract == null)
                return false;

            contract.Status = newStatus;
            contract.UpdatedAt = DateTime.UtcNow;

            await _repo.UpdateAsync(contract);
            await _repo.SaveChangesAsync();

            return true;
        }

    }
}
