using System.Net.Http.Headers;
using TechMove.GLMS.Services;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------
// 1. MVC + Views
// ---------------------------------------------------------
builder.Services.AddControllersWithViews();

// ---------------------------------------------------------
// 2. Add Session (to store JWT token)
// ---------------------------------------------------------
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ---------------------------------------------------------
// 3. Add HttpClient for API calls
// ---------------------------------------------------------
builder.Services.AddHttpClient("API", client =>
{
    var apiBaseAddress = builder.Configuration["ApiSettings:BaseAddress"] ?? "https://localhost:7038/";
    client.BaseAddress = new Uri(apiBaseAddress);

    client.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/json"));
});

// ---------------------------------------------------------
// 4. Custom Services
// ---------------------------------------------------------
builder.Services.AddHttpClient<CurrencyService>();
builder.Services.AddScoped<FileService>();
builder.Services.AddScoped<WorkflowService>();

var app = builder.Build();

// ---------------------------------------------------------
// 5. Middleware Pipeline
// ---------------------------------------------------------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Enable session BEFORE authorization
app.UseSession();

app.Use(async (context, next) =>
{
    var token = context.Session.GetString("JWT");

    if (!string.IsNullOrEmpty(token))
    {
        var clientFactory = context.RequestServices.GetRequiredService<IHttpClientFactory>();
        var client = clientFactory.CreateClient("API");

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    await next();
});

app.UseAuthorization();

// ---------------------------------------------------------
// 6. MVC Routing
// ---------------------------------------------------------
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

// Expose Program for WebApplicationFactory
namespace TechMove.GLMS
{
    public partial class Program { }
}
