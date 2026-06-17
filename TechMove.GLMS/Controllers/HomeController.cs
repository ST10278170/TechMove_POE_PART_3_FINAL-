using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using TechMove.GLMS.API.Models;             // Client, Contract, ServiceRequest
using TechMove.GLMS.API.Models.Enums;       // ContractStatus, ServiceRequestStatus
using TechMove.GLMS.ViewModels;             // DashboardViewModel

namespace TechMove.GLMS.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public HomeController(ILogger<HomeController> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        private HttpClient CreateClient()
        {
            var client = _httpClientFactory.CreateClient("API");

            var token = HttpContext.Session.GetString("JWT");
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }

            return client;
        }

        public async Task<IActionResult> Index()
        {
            var client = CreateClient();
            var dashboard = new DashboardViewModel();

            // -----------------------------
            // 1. Load Clients
            // -----------------------------
            try
            {
                var clientsResponse = await client.GetAsync("api/clients");
                if (clientsResponse.IsSuccessStatusCode)
                {
                    var json = await clientsResponse.Content.ReadAsStringAsync();
                    var clients = JsonConvert.DeserializeObject<List<Client>>(json) ?? new List<Client>();
                    dashboard.TotalClients = clients.Count;
                }
            }
            catch { }

            // -----------------------------
            // 2. Load Contracts
            // -----------------------------
            try
            {
                var contractsResponse = await client.GetAsync("api/contracts");
                if (contractsResponse.IsSuccessStatusCode)
                {
                    var json = await contractsResponse.Content.ReadAsStringAsync();
                    var contracts = JsonConvert.DeserializeObject<List<Contract>>(json) ?? new List<Contract>();

                    dashboard.TotalContracts = contracts.Count;
                    dashboard.ActiveContracts = contracts.Count(c => c.Status == ContractStatus.Active);
                    dashboard.ExpiredContracts = contracts.Count(c => c.Status == ContractStatus.Expired);
                    dashboard.OnHoldContracts = contracts.Count(c => c.Status == ContractStatus.OnHold);

                    // Last 6 months labels
                    for (int i = 5; i >= 0; i--)
                    {
                        var month = DateTime.Now.AddMonths(-i);
                        dashboard.Last6MonthsLabels.Add(month.ToString("MMM"));
                    }

                    // Contracts per month
                    dashboard.ContractsPerMonth = Enumerable.Range(0, 6)
                        .Select(i =>
                        {
                            var month = DateTime.Now.AddMonths(-i);
                            return contracts.Count(c =>
                                c.StartDate.Month == month.Month &&
                                c.StartDate.Year == month.Year);
                        })
                        .ToList();

                    // Expiring contracts
                    var today = DateTime.Today;
                    dashboard.ExpiringContracts = contracts
                        .Where(c => c.EndDate >= today && c.EndDate <= today.AddDays(30))
                        .ToList();
                }
            }
            catch { }

            // -----------------------------
            // 3. Load Service Requests
            // -----------------------------
            try
            {
                var requestsResponse = await client.GetAsync("api/servicerequests");
                if (requestsResponse.IsSuccessStatusCode)
                {
                    var json = await requestsResponse.Content.ReadAsStringAsync();
                    var requests = JsonConvert.DeserializeObject<List<ServiceRequest>>(json) ?? new List<ServiceRequest>();

                    dashboard.TotalServiceRequests = requests.Count;
                    dashboard.PendingRequests = requests.Count(r => r.Status == ServiceRequestStatus.Pending);
                    dashboard.CompletedRequests = requests.Count(r => r.Status == ServiceRequestStatus.Completed);

                    // Requests per month
                    dashboard.RequestsPerMonth = Enumerable.Range(0, 6)
                        .Select(i =>
                        {
                            var month = DateTime.Now.AddMonths(-i);
                            return requests.Count(r =>
                                r.CreatedAt.Month == month.Month &&
                                r.CreatedAt.Year == month.Year);
                        })
                        .ToList();

                    // Recent 5 requests
                    dashboard.RecentRequests = requests
                        .OrderByDescending(r => r.Id)
                        .Take(5)
                        .ToList();
                }
            }
            catch { }

            return View(dashboard);
        }
    }
}
