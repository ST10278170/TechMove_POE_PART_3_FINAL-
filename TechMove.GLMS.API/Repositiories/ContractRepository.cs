using Microsoft.EntityFrameworkCore;
using TechMove.GLMS.API.Data;
using TechMove.GLMS.API.Models;
using TechMove.GLMS.API.Models.Enums;

namespace TechMove.GLMS.API.Repositories
{
    public class ContractRepository : IContractRepository
    {
        private readonly AppDbContext _context;

        public ContractRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Contract>> GetAllAsync(string? status = null)
        {
            var query = _context.Contracts.AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                if (Enum.TryParse<ContractStatus>(status, true, out var parsedStatus))
                {
                    query = query.Where(c => c.Status == parsedStatus);
                }
            }

            return await query.ToListAsync();
        }

        public async Task<Contract?> GetByIdAsync(int id)
        {
            return await _context.Contracts
                .Include(c => c.ServiceRequests)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task AddAsync(Contract contract)
        {
            await _context.Contracts.AddAsync(contract);
        }

        public async Task UpdateAsync(Contract contract)
        {
            _context.Contracts.Update(contract);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
