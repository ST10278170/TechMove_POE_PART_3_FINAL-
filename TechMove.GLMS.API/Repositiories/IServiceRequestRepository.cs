using TechMove.GLMS.API.Models;

namespace TechMove.GLMS.API.Repositories
{
    public interface IServiceRequestRepository
    {
        Task<IEnumerable<ServiceRequest>> GetAllAsync();
        Task<ServiceRequest?> GetByIdAsync(int id);
        Task AddAsync(ServiceRequest request);

        // ⭐ NEW: Required for DELETE
        Task DeleteAsync(ServiceRequest request);

        Task SaveChangesAsync();
    }
}
