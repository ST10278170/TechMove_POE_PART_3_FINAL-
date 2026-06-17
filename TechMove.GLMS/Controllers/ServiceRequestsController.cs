using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using TechMove.GLMS.Helpers;                 // PaginatedList
using TechMove.GLMS.API.Models;             // ServiceRequest, Contract
using TechMove.GLMS.API.Models.Enums;       // ServiceRequestStatus, ContractStatus
using TechMove.GLMS.Services;               // CurrencyService, WorkflowService

namespace TechMove.GLMS.Controllers
{
    public class ServiceRequestsController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly CurrencyService _currencyService;
        private readonly WorkflowService _workflowService;

        public ServiceRequestsController(
            IHttpClientFactory httpClientFactory,
            CurrencyService currencyService,
            WorkflowService workflowService)
        {
            _httpClientFactory = httpClientFactory;
            _currencyService = currencyService;
            _workflowService = workflowService;
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

        // GET: ServiceRequests
        public async Task<IActionResult> Index(string sortOrder, string search, int pageNumber = 1)
        {
            var client = CreateClient();

            var response = await client.GetAsync("api/servicerequests");
            if (!response.IsSuccessStatusCode)
                return View(new PaginatedList<ServiceRequest>(new List<ServiceRequest>(), 0, pageNumber, 10));

            var json = await response.Content.ReadAsStringAsync();
            var requests = JsonConvert.DeserializeObject<List<ServiceRequest>>(json) ?? new List<ServiceRequest>();

            // SEARCH
            if (!string.IsNullOrEmpty(search))
            {
                requests = requests
                    .Where(r => r.Description.Contains(search, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            // SORTING
            ViewData["IdSort"] = sortOrder == "id_asc" ? "id_desc" : "id_asc";
            ViewData["StatusSort"] = sortOrder == "status_asc" ? "status_desc" : "status_asc";
            ViewData["CreatedSort"] = sortOrder == "created_asc" ? "created_desc" : "created_asc";

            requests = sortOrder switch
            {
                "id_desc" => requests.OrderByDescending(r => r.Id).ToList(),
                "id_asc" => requests.OrderBy(r => r.Id).ToList(),
                "status_desc" => requests.OrderByDescending(r => r.Status).ToList(),
                "status_asc" => requests.OrderBy(r => r.Status).ToList(),
                "created_desc" => requests.OrderByDescending(r => r.CreatedAt).ToList(),
                "created_asc" => requests.OrderBy(r => r.CreatedAt).ToList(),
                _ => requests.OrderBy(r => r.Id).ToList()
            };

            // PAGINATION
            int pageSize = 10;
            return View(PaginatedList<ServiceRequest>.Create(requests, pageNumber, pageSize));
        }

        // GET: ServiceRequests/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var client = CreateClient();

            var response = await client.GetAsync($"api/servicerequests/{id}");
            if (!response.IsSuccessStatusCode)
                return NotFound();

            var json = await response.Content.ReadAsStringAsync();
            var request = JsonConvert.DeserializeObject<ServiceRequest>(json);

            return View(request);
        }

        // GET: ServiceRequests/Create
        public async Task<IActionResult> Create()
        {
            await LoadDropdowns(0, ServiceRequestStatus.Pending);
            return View();
        }

        // POST: ServiceRequests/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ServiceRequest model)
        {
            ModelState.Remove("CostZAR");

            if (model.ContractId <= 0)
                ModelState.AddModelError(nameof(model.ContractId), "Please select a contract.");

            if (!ModelState.IsValid)
            {
                await LoadDropdowns(model.ContractId, model.Status);
                return View(model);
            }

            model.CostZAR = model.CostUSD * 16.70m;
            model.CreatedAt = DateTime.Now;

            var client = CreateClient();
            var response = await client.PostAsJsonAsync("api/servicerequests", model);

            if (!response.IsSuccessStatusCode)
            {
                await LoadDropdowns(model.ContractId, model.Status);
                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: ServiceRequests/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var client = CreateClient();

            var response = await client.GetAsync($"api/servicerequests/{id}");
            if (!response.IsSuccessStatusCode)
                return NotFound();

            var json = await response.Content.ReadAsStringAsync();
            var model = JsonConvert.DeserializeObject<ServiceRequest>(json);

            await LoadDropdowns(model.ContractId, model.Status);

            return View(model);
        }

        // POST: ServiceRequests/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ServiceRequest model)
        {
            if (id != model.Id)
                return NotFound();

            ModelState.Remove("CostZAR");

            if (model.ContractId <= 0)
                ModelState.AddModelError(nameof(model.ContractId), "Please select a contract.");

            if (!ModelState.IsValid)
            {
                await LoadDropdowns(model.ContractId, model.Status);
                return View(model);
            }

            // Workflow validation
            var client = CreateClient();
            var contractResponse = await client.GetAsync($"api/contracts/{model.ContractId}");

            if (!contractResponse.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "Contract not found.");
                await LoadDropdowns(model.ContractId, model.Status);
                return View(model);
            }

            var contractJson = await contractResponse.Content.ReadAsStringAsync();
            var contract = JsonConvert.DeserializeObject<Contract>(contractJson);

            _workflowService.UpdateContractStatus(contract);

            if (!_workflowService.CanCreateServiceRequest(contract))
            {
                ModelState.AddModelError("", "Cannot modify a service request for an expired contract.");
                await LoadDropdowns(model.ContractId, model.Status);
                return View(model);
            }

            model.CostZAR = await _currencyService.ConvertUsdToZarAsync(model.CostUSD);

            var updateResponse = await client.PutAsJsonAsync($"api/servicerequests/{id}", model);

            if (!updateResponse.IsSuccessStatusCode)
            {
                await LoadDropdowns(model.ContractId, model.Status);
                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: ServiceRequests/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var client = CreateClient();

            var response = await client.GetAsync($"api/servicerequests/{id}");
            if (!response.IsSuccessStatusCode)
                return NotFound();

            var json = await response.Content.ReadAsStringAsync();
            var request = JsonConvert.DeserializeObject<ServiceRequest>(json);

            return View(request);
        }

        // POST: ServiceRequests/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var client = CreateClient();
            await client.DeleteAsync($"api/servicerequests/{id}");

            return RedirectToAction(nameof(Index));
        }

        // Helper: Load dropdowns
        private async Task LoadDropdowns(int selectedContractId, ServiceRequestStatus status)
        {
            var client = CreateClient();

            var contractsResponse = await client.GetAsync("api/contracts");
            var contracts = new List<Contract>();

            if (contractsResponse.IsSuccessStatusCode)
            {
                var json = await contractsResponse.Content.ReadAsStringAsync();
                contracts = JsonConvert.DeserializeObject<List<Contract>>(json)
                    .Where(c => c.Status == ContractStatus.Active
                             || c.Status == ContractStatus.Pending
                             || c.Status == ContractStatus.Future
                             || c.Status == ContractStatus.OnHold)
                    .ToList();
            }

            ViewBag.Contracts = contracts.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = $"Contract #{c.Id} (Client {c.ClientId})",
                Selected = c.Id == selectedContractId
            }).ToList();

            ViewBag.Status = new SelectList(Enum.GetValues(typeof(ServiceRequestStatus)), status);
        }
    }
}
