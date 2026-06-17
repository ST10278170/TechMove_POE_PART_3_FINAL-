using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using TechMove.GLMS.Helpers;
using TechMove.GLMS.API.Models;

namespace TechMove.GLMS.Controllers
{
    public class ClientsController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ClientsController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // Helper: Create HttpClient with JWT
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

        // GET: Clients
        public async Task<IActionResult> Index(string sortOrder, string searchString, int pageNumber = 1)
        {
            var client = CreateClient();

            var response = await client.GetAsync("api/clients");
            if (!response.IsSuccessStatusCode)
                return View(new PaginatedList<Client>(new List<Client>(), 0, pageNumber, 10));

            var json = await response.Content.ReadAsStringAsync();
            var clients = JsonConvert.DeserializeObject<List<Client>>(json);

            // SEARCH
            if (!string.IsNullOrEmpty(searchString))
            {
                clients = clients
                    .Where(c => c.Name.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            // SORTING
            ViewData["NameSort"] = sortOrder == "name_asc" ? "name_desc" : "name_asc";
            ViewData["RegionSort"] = sortOrder == "region_asc" ? "region_desc" : "region_asc";

            clients = sortOrder switch
            {
                "name_desc" => clients.OrderByDescending(c => c.Name).ToList(),
                "name_asc" => clients.OrderBy(c => c.Name).ToList(),
                "region_desc" => clients.OrderByDescending(c => c.Region).ToList(),
                "region_asc" => clients.OrderBy(c => c.Region).ToList(),
                _ => clients.OrderBy(c => c.Name).ToList()
            };

            // PAGINATION
            int pageSize = 10;
            return View(PaginatedList<Client>.Create(clients, pageNumber, pageSize));
        }

        // GET: Clients/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var client = CreateClient();

            var response = await client.GetAsync($"api/clients/{id}");
            if (!response.IsSuccessStatusCode)
                return NotFound();

            var json = await response.Content.ReadAsStringAsync();
            var model = JsonConvert.DeserializeObject<Client>(json);

            return View(model);
        }

        // GET: Clients/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Clients/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Client clientModel)
        {
            if (!ModelState.IsValid)
                return View(clientModel);

            var client = CreateClient();
            var response = await client.PostAsJsonAsync("api/clients", clientModel);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "API rejected the request.");
                return View(clientModel);
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Clients/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var client = CreateClient();

            var response = await client.GetAsync($"api/clients/{id}");
            if (!response.IsSuccessStatusCode)
                return NotFound();

            var json = await response.Content.ReadAsStringAsync();
            var model = JsonConvert.DeserializeObject<Client>(json);

            return View(model);
        }

        // POST: Clients/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Client clientModel)
        {
            if (id != clientModel.Id)
                return NotFound();

            if (!ModelState.IsValid)
                return View(clientModel);

            var client = CreateClient();
            var response = await client.PutAsJsonAsync($"api/clients/{id}", clientModel);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "API rejected the update.");
                return View(clientModel);
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Clients/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var client = CreateClient();

            var response = await client.GetAsync($"api/clients/{id}");
            if (!response.IsSuccessStatusCode)
                return NotFound();

            var json = await response.Content.ReadAsStringAsync();
            var model = JsonConvert.DeserializeObject<Client>(json);

            return View(model);
        }

        // POST: Clients/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var client = CreateClient();
            await client.DeleteAsync($"api/clients/{id}");

            return RedirectToAction(nameof(Index));
        }
    }
}
