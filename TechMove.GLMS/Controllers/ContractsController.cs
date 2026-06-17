using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using TechMove.GLMS.Helpers;                 // PaginatedList
using TechMove.GLMS.API.Models;             // Contract, Client, ServiceRequest
using TechMove.GLMS.API.Models.Enums;       // ContractStatus
using TechMove.GLMS.Services;               // FileService

namespace TechMove.GLMS.Controllers
{
    public class ContractsController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly FileService _fileService;

        public ContractsController(IHttpClientFactory httpClientFactory, FileService fileService)
        {
            _httpClientFactory = httpClientFactory;
            _fileService = fileService;
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

        // GET: Contracts
        public async Task<IActionResult> Index(
            string sortOrder,
            string search,
            DateTime? startDate,
            DateTime? endDate,
            ContractStatus? status,
            int pageNumber = 1)
        {
            var client = CreateClient();

            var response = await client.GetAsync("api/contracts");
            if (!response.IsSuccessStatusCode)
                return View(new PaginatedList<Contract>(new List<Contract>(), 0, pageNumber, 10));

            var json = await response.Content.ReadAsStringAsync();
            var contracts = JsonConvert.DeserializeObject<List<Contract>>(json) ?? new List<Contract>();

            // SEARCH
            if (!string.IsNullOrEmpty(search))
            {
                contracts = contracts
                    .Where(c =>
                        c.ClientId.ToString().Contains(search) ||
                        c.ServiceLevel.Contains(search, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            // DATE FILTERS
            if (startDate.HasValue)
                contracts = contracts.Where(c => c.StartDate >= startDate.Value).ToList();

            if (endDate.HasValue)
                contracts = contracts.Where(c => c.EndDate <= endDate.Value).ToList();

            // STATUS FILTER
            if (status.HasValue)
                contracts = contracts.Where(c => c.Status == status.Value).ToList();

            // SORTING
            ViewData["ClientSort"] = sortOrder == "client_asc" ? "client_desc" : "client_asc";
            ViewData["StatusSort"] = sortOrder == "status_asc" ? "status_desc" : "status_asc";
            ViewData["StartSort"] = sortOrder == "start_asc" ? "start_desc" : "start_asc";

            contracts = sortOrder switch
            {
                "client_desc" => contracts.OrderByDescending(c => c.ClientId).ToList(),
                "client_asc" => contracts.OrderBy(c => c.ClientId).ToList(),
                "status_desc" => contracts.OrderByDescending(c => c.Status).ToList(),
                "status_asc" => contracts.OrderBy(c => c.Status).ToList(),
                "start_desc" => contracts.OrderByDescending(c => c.StartDate).ToList(),
                "start_asc" => contracts.OrderBy(c => c.StartDate).ToList(),
                _ => contracts.OrderBy(c => c.ClientId).ToList()
            };

            // PAGINATION
            int pageSize = 10;
            return View(PaginatedList<Contract>.Create(contracts, pageNumber, pageSize));
        }

        // GET: Contracts/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var client = CreateClient();

            var response = await client.GetAsync($"api/contracts/{id}");
            if (!response.IsSuccessStatusCode)
                return NotFound();

            var json = await response.Content.ReadAsStringAsync();
            var contract = JsonConvert.DeserializeObject<Contract>(json);

            return View(contract);
        }

        // GET: Contracts/Create
        public IActionResult Create()
        {
            ViewBag.Status = new SelectList(Enum.GetValues(typeof(ContractStatus)));
            return View(new Contract());
        }

        // POST: Contracts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Contract model, IFormFile? signedAgreement)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Status = new SelectList(Enum.GetValues(typeof(ContractStatus)), model.Status);
                return View(model);
            }

            if (signedAgreement != null)
            {
                model.SignedAgreementPath = await _fileService.SavePdfAsync(signedAgreement);
            }

            var client = CreateClient();
            var response = await client.PostAsJsonAsync("api/contracts", model);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "API rejected the request.");
                ViewBag.Status = new SelectList(Enum.GetValues(typeof(ContractStatus)), model.Status);
                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Contracts/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var client = CreateClient();

            var response = await client.GetAsync($"api/contracts/{id}");
            if (!response.IsSuccessStatusCode)
                return NotFound();

            var json = await response.Content.ReadAsStringAsync();
            var contract = JsonConvert.DeserializeObject<Contract>(json);

            ViewBag.Status = new SelectList(Enum.GetValues(typeof(ContractStatus)), contract.Status);

            return View(contract);
        }

        // POST: Contracts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Contract model, IFormFile? signedAgreement)
        {
            if (id != model.Id)
                return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.Status = new SelectList(Enum.GetValues(typeof(ContractStatus)), model.Status);
                return View(model);
            }

            if (signedAgreement != null)
            {
                model.SignedAgreementPath = await _fileService.SavePdfAsync(signedAgreement);
            }

            var client = CreateClient();
            var response = await client.PutAsJsonAsync($"api/contracts/{id}", model);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "API rejected the update.");
                ViewBag.Status = new SelectList(Enum.GetValues(typeof(ContractStatus)), model.Status);
                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Contracts/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var client = CreateClient();

            var response = await client.GetAsync($"api/contracts/{id}");
            if (!response.IsSuccessStatusCode)
                return NotFound();

            var json = await response.Content.ReadAsStringAsync();
            var contract = JsonConvert.DeserializeObject<Contract>(json);

            return View(contract);
        }

        // POST: Contracts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var client = CreateClient();
            await client.DeleteAsync($"api/contracts/{id}");

            return RedirectToAction(nameof(Index));
        }

        // DOWNLOAD PDF
        public IActionResult Download(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return NotFound();

            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", filePath.TrimStart('/'));

            if (!System.IO.File.Exists(fullPath))
                return NotFound();

            var fileBytes = System.IO.File.ReadAllBytes(fullPath);
            return File(fileBytes, "application/pdf", Path.GetFileName(fullPath));
        }
    }
}
