using Microsoft.EntityFrameworkCore;
using TechMove.GLMS.API.Data;
using TechMove.GLMS.API.Repositories;
using TechMove.GLMS.API.Services;
using TechMove.GLMS.API.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TechMove.GLMS.API.Helpers;
using TechMove.GLMS.API.Middleware;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------
// 1. DbContext
// ---------------------------------------------------------
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ---------------------------------------------------------
// 2. Controllers
// ---------------------------------------------------------
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

// ---------------------------------------------------------
// 3. Swagger
// ---------------------------------------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ---------------------------------------------------------
// 4. Dependency Injection – Repositories
// ---------------------------------------------------------
builder.Services.AddScoped<IContractRepository, ContractRepository>();
builder.Services.AddScoped<IServiceRequestRepository, ServiceRequestRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// ---------------------------------------------------------
// 5. Dependency Injection – Services
// ---------------------------------------------------------
builder.Services.AddScoped<IContractService, ContractService>();
builder.Services.AddScoped<IServiceRequestService, ServiceRequestService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// ---------------------------------------------------------
// 6. JWT Token Generator
// ---------------------------------------------------------
builder.Services.AddSingleton<JwtTokenGenerator>();

// ---------------------------------------------------------
// 7. JWT Authentication
// ---------------------------------------------------------
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        ClockSkew = TimeSpan.Zero
    };
});

// ---------------------------------------------------------
// Build App
// ---------------------------------------------------------
var app = builder.Build();

// ---------------------------------------------------------
// 8. Swagger (Dev + Prod)
// ---------------------------------------------------------
app.UseSwagger();
app.UseSwaggerUI();

// ---------------------------------------------------------
// 9. Middleware Pipeline
// ---------------------------------------------------------
if (!app.Environment.IsEnvironment("Testing"))
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseMiddleware<JwtMiddleware>();
app.UseAuthorization();

// ---------------------------------------------------------
// 10. Map Controllers
// ---------------------------------------------------------
app.MapControllers();

// ---------------------------------------------------------
// 11. Database Initialization & Seeding
// ---------------------------------------------------------
if (!app.Environment.IsEnvironment("Testing"))
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var context = services.GetRequiredService<AppDbContext>();
            
            // Ensure Database is Created
            context.Database.EnsureCreated();

            // Seed Users
            if (!context.Users.Any())
            {
                context.Users.Add(new User
                {
                    Username = "admin",
                    PasswordHash = "admin123", // Matches plain text password check in AuthService
                    Role = "Admin"
                });
                context.SaveChanges();
            }

            // Seed Clients
            if (!context.Clients.Any())
            {
                context.Clients.AddRange(
                    new Client
                    {
                        Name = "Acme Corp",
                        ContactPerson = "John Doe",
                        Email = "john@acme.com",
                        Phone = "+27 11 123 4567",
                        Region = "Gauteng",
                        Address = "123 Main St, Johannesburg",
                        CompanyRegistrationNumber = "2024/123456/07",
                        VATNumber = "4990123456",
                        Notes = "Key enterprise account"
                    },
                    new Client
                    {
                        Name = "Globex International",
                        ContactPerson = "Jane Smith",
                        Email = "jane@globex.com",
                        Phone = "+27 21 987 6543",
                        Region = "Western Cape",
                        Address = "456 Broad Rd, Cape Town",
                        CompanyRegistrationNumber = "2023/654321/07",
                        VATNumber = "4990654321",
                        Notes = "Growing mid-market client"
                    }
                );
                context.SaveChanges();
            }

            // Seed Contracts
            if (!context.Contracts.Any())
            {
                var acme = context.Clients.FirstOrDefault(c => c.Name == "Acme Corp");
                var globex = context.Clients.FirstOrDefault(c => c.Name == "Globex International");

                if (acme != null)
                {
                    context.Contracts.Add(new Contract
                    {
                        ContractNumber = "CON-2026-001",
                        ClientId = acme.Id,
                        ClientName = acme.Name,
                        ServiceLevel = "Gold",
                        StartDate = DateTime.UtcNow.AddMonths(-1),
                        EndDate = DateTime.UtcNow.AddMonths(11),
                        Status = TechMove.GLMS.API.Models.Enums.ContractStatus.Active,
                        BaseRate = 12000.00m,
                        PenaltyRate = 150.00m,
                        CreatedAt = DateTime.UtcNow
                    });
                }

                if (globex != null)
                {
                    context.Contracts.Add(new Contract
                    {
                        ContractNumber = "CON-2026-002",
                        ClientId = globex.Id,
                        ClientName = globex.Name,
                        ServiceLevel = "Silver",
                        StartDate = DateTime.UtcNow.AddMonths(-12),
                        EndDate = DateTime.UtcNow.AddMonths(-1),
                        Status = TechMove.GLMS.API.Models.Enums.ContractStatus.Expired,
                        BaseRate = 8000.00m,
                        PenaltyRate = 100.00m,
                        CreatedAt = DateTime.UtcNow.AddMonths(-12)
                    });
                }

                context.SaveChanges();
            }
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred while creating or seeding the database.");
        }
    }
}

app.Run();

namespace TechMove.GLMS.API
{
    public partial class Program { }
}
