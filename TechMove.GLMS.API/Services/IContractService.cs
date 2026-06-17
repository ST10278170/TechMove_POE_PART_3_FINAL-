using TechMove.GLMS.API.DTOs;
using TechMove.GLMS.API.Models;
using TechMove.GLMS.API.Models.Enums;

namespace TechMove.GLMS.API.Services
{
    public interface IContractService
    {
        Task<IEnumerable<Contract>> GetAllAsync(string? status = null);
        Task<Contract?> GetByIdAsync(int id);
        Task<Contract> CreateAsync(ContractCreateDto dto);
        Task<bool> UpdateStatusAsync(int id, ContractStatus newStatus);


    }
}
