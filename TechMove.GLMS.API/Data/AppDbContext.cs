using Microsoft.EntityFrameworkCore;
using TechMove.GLMS.API.Models;

namespace TechMove.GLMS.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

      
        public DbSet<Contract> Contracts { get; set; }
        public DbSet<ServiceRequest> ServiceRequests { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Client> Clients { get; set; }
    }
}
