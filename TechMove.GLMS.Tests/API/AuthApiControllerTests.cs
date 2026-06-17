using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.Net.Http.Json;
using TechMove.GLMS.API.DTOs;

namespace TechMove.GLMS.Tests.API
{
    [TestClass]
    public class AuthApiControllerTests
    {
        private readonly ApiWebApplicationFactory _factory = new();

        // POST /api/auth/login
        [TestMethod]
        public async Task Login_ReturnsSuccessOrUnauthorized()
        {
            var client = _factory.CreateClient();

            var loginRequest = new AuthRequestDto
            {
                Username = "admin",
                Password = "password123"
            };

            var response = await client.PostAsJsonAsync("/api/auth/login", loginRequest);

            Assert.IsTrue(
                response.StatusCode == HttpStatusCode.OK ||
                response.StatusCode == HttpStatusCode.Unauthorized,
                $"Expected OK or Unauthorized, got {response.StatusCode}");
        }

        // POST /api/auth/register
        [TestMethod]
        public async Task Register_ReturnsCreatedOrBadRequest()
        {
            var client = _factory.CreateClient();

            var newUser = new RegisterDto
            {
                Username = "newuser",
                Password = "test123",
                Role = "User"
            };

            var response = await client.PostAsJsonAsync("/api/auth/register", newUser);

            Assert.IsTrue(
                response.StatusCode == HttpStatusCode.Created ||
                response.StatusCode == HttpStatusCode.BadRequest,
                $"Expected Created or BadRequest, got {response.StatusCode}");
        }

        // POST /api/auth/login with invalid credentials
        [TestMethod]
        public async Task Login_InvalidCredentials_ReturnsUnauthorized()
        {
            var client = _factory.CreateClient();

            var loginRequest = new AuthRequestDto
            {
                Username = "wronguser",
                Password = "wrongpassword"
            };

            var response = await client.PostAsJsonAsync("/api/auth/login", loginRequest);

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}
