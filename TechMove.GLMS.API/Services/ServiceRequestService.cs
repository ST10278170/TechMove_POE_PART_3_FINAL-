using TechMove.GLMS.API.DTOs;
using TechMove.GLMS.API.Models;
using TechMove.GLMS.API.Models.Enums;
using TechMove.GLMS.API.Repositories;

namespace TechMove.GLMS.API.Services
{
    public class ServiceRequestService : IServiceRequestService
    {
        private readonly IServiceRequestRepository _repo;
        private readonly IContractRepository _contractRepo;

        public ServiceRequestService(IServiceRequestRepository repo, IContractRepository contractRepo)
        {
            _repo = repo;
            _contractRepo = contractRepo;
        }

        public async Task<IEnumerable<ServiceRequest>> GetAllAsync()
        {
            return await _repo.GetAllAsync();
        }

        public async Task<ServiceRequest?> GetByIdAsync(int id)
        {
            return await _repo.GetByIdAsync(id);
        }

        public async Task<ServiceRequest> CreateAsync(ServiceRequestCreateDto dto)
        {
            var contract = await _contractRepo.GetByIdAsync(dto.ContractId);

            if (contract == null)
                throw new Exception("Contract not found.");

            // FIXED: Compare enum to enum
            if (contract.Status == ContractStatus.Expired)
                throw new Exception("Cannot create service request for expired contract.");

            var request = new ServiceRequest
            {
                ContractId = dto.ContractId,
                Description = dto.Description,
                CostUSD = dto.CostUSD,
                CostZAR = dto.CostUSD * 18.5m,

                // FIXED: Status must be enum
                Status = ServiceRequestStatus.InProgress
            };

            await _repo.AddAsync(request);
            await _repo.SaveChangesAsync();

            return request;
        }

        public async Task<bool> UpdateAsync(int id, ServiceRequestUpdateDto dto)
        {
            var request = await _repo.GetByIdAsync(id);
            if (request == null)
                return false;

            request.Description = dto.Description;

            // FIXED: dto.Status is now enum (after your earlier fix)
            request.Status = dto.Status;

            request.CostUSD = dto.CostUSD;
            request.CostZAR = dto.CostZAR;

            await _repo.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var request = await _repo.GetByIdAsync(id);
            if (request == null)
                return false;

            await _repo.DeleteAsync(request);
            await _repo.SaveChangesAsync();
            return true;
        }
    }
}
