using TechMove.GLMS.API.Models;

namespace TechMove.GLMS.API.Repositories
{
    public interface IContractRepository
    {
        Task<IEnumerable<Contract>> GetAllAsync(string? status = null);
        Task<Contract?> GetByIdAsync(int id);
        Task AddAsync(Contract contract);
        Task UpdateAsync(Contract contract);
        Task SaveChangesAsync();
    }
}
