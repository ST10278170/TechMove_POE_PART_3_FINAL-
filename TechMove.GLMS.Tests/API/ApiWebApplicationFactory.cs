using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TechMove.GLMS.API.Data;
using TechMove.GLMS.API.Models;
using TechMove.GLMS.API.Models.Enums;
using ApiProgram = TechMove.GLMS.API.Program;

namespace TechMove.GLMS.Tests.API
{
    public class ApiWebApplicationFactory : WebApplicationFactory<ApiProgram>
    {
        private readonly string _dbName = Guid.NewGuid().ToString();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Remove existing DbContext
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

                if (descriptor != null)
                    services.Remove(descriptor);

                // Add isolated InMemory DB for API tests
                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseInMemoryDatabase(_dbName);
                });

                // Build provider and seed test data
                var sp = services.BuildServiceProvider();

                using (var scope = sp.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    db.Database.EnsureCreated();

                    SeedTestData(db);
                }
            });
        }

        private void SeedTestData(AppDbContext db)
        {
            if (!db.Contracts.Any())
            {
                db.Contracts.Add(new Contract
                {
                    Id = 1,
                    ClientId = 1,
                    ServiceLevel = "Gold",
                    Status = ContractStatus.Active,
                    StartDate = DateTime.UtcNow.AddDays(-10),
                    EndDate = DateTime.UtcNow.AddDays(10),
                    ContractNumber = "CNT-001"
                });
            }

            if (!db.ServiceRequests.Any())
            {
                db.ServiceRequests.Add(new ServiceRequest
                {
                    Id = 1,
                    ContractId = 1,
                    Description = "Seeded Request",
                    Status = ServiceRequestStatus.Pending,
                    CostUSD = 50,
                    CostZAR = 900,
                    CreatedAt = DateTime.UtcNow
                });
            }

            db.SaveChanges();
        }
    }
}
