using Microsoft.EntityFrameworkCore;
using TechMove.GLMS.API.Data;
using TechMove.GLMS.API.Models;

namespace TechMove.GLMS.API.Repositories
{
    public class ServiceRequestRepository : IServiceRequestRepository
    {
        private readonly AppDbContext _context;

        public ServiceRequestRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ServiceRequest>> GetAllAsync()
        {
            return await _context.ServiceRequests
                .Include(sr => sr.Contract)
                .ToListAsync();
        }

        public async Task<ServiceRequest?> GetByIdAsync(int id)
        {
            return await _context.ServiceRequests
                .Include(sr => sr.Contract)
                .FirstOrDefaultAsync(sr => sr.Id == id);
        }

        public async Task AddAsync(ServiceRequest request)
        {
            await _context.ServiceRequests.AddAsync(request);
        }

        // ⭐ NEW: Required for DELETE
        public async Task DeleteAsync(ServiceRequest request)
        {
            _context.ServiceRequests.Remove(request);
            await Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
