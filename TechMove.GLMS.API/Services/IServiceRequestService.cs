using TechMove.GLMS.API.DTOs;
using TechMove.GLMS.API.Models;

namespace TechMove.GLMS.API.Services
{
    public interface IServiceRequestService
    {
        Task<IEnumerable<ServiceRequest>> GetAllAsync();
        Task<ServiceRequest?> GetByIdAsync(int id);
        Task<ServiceRequest> CreateAsync(ServiceRequestCreateDto dto);

        // Newly added for PUT + DELETE
        Task<bool> UpdateAsync(int id, ServiceRequestUpdateDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
