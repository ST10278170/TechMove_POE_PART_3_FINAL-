using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using TechMove.GLMS.API.DTOs;          // AuthResponseDto, AuthRequestDto
using TechMove.GLMS.ViewModels;       // LoginViewModel

namespace TechMove.GLMS.Controllers
{
    public class AuthController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AuthController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        private HttpClient CreateClient()
        {
            var client = _httpClientFactory.CreateClient("API");
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            return client;
        }

        // GET: /Auth/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View(new LoginViewModel());
        }

        // POST: /Auth/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var client = CreateClient();

            var response = await client.PostAsJsonAsync("api/auth/login", model);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "Invalid username or password.");
                return View(model);
            }

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<AuthResponseDto>(json);

            if (result == null || string.IsNullOrEmpty(result.Token))
            {
                ModelState.AddModelError("", "Authentication failed.");
                return View(model);
            }

            HttpContext.Session.SetString("JWT", result.Token);

            return RedirectToAction("Index", "Home");
        }

        // GET: /Auth/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("JWT");
            return RedirectToAction("Login");
        }
    }
}
