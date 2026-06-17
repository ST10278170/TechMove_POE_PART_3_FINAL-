using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.Net.Http.Json;
using TechMove.GLMS.API.DTOs;
using TechMove.GLMS.API.Models;
using TechMove.GLMS.API.Models.Enums;

namespace TechMove.GLMS.Tests.API
{
    [TestClass]
    public class ContractsApiControllerTests
    {
        private readonly ApiWebApplicationFactory _factory = new();

        // GET /api/contracts
        [TestMethod]
        public async Task GetAll_ReturnsSuccess()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("/api/contracts");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        // GET /api/contracts/1
        [TestMethod]
        public async Task GetById_ReturnsContract()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("/api/contracts/1");

            Assert.IsTrue(
                response.StatusCode == HttpStatusCode.OK ||
                response.StatusCode == HttpStatusCode.NotFound,
                $"Expected OK or NotFound, got {response.StatusCode}");

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var data = await response.Content.ReadFromJsonAsync<Contract>();
                Assert.IsNotNull(data);
                Assert.AreEqual(1, data.Id);
            }
        }

        // POST /api/contracts
        [TestMethod]
        public async Task Create_ReturnsCreatedOrBadRequest()
        {
            var client = _factory.CreateClient();

            var newContract = new ContractCreateDto
            {
                ClientId = 1,
                ServiceLevel = "Silver",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(30)
            };

            var response = await client.PostAsJsonAsync("/api/contracts", newContract);

            Assert.IsTrue(
                response.StatusCode == HttpStatusCode.Created ||
                response.StatusCode == HttpStatusCode.BadRequest,
                $"Expected Created or BadRequest, got {response.StatusCode}");
        }

        // PATCH /api/contracts/1/status
        [TestMethod]
        public async Task UpdateStatus_ReturnsSuccessOrNotFound()
        {
            var client = _factory.CreateClient();

            var updateDto = new ContractUpdateStatusDto
            {
                Status = ContractStatus.Active
            };

            var response = await client.PatchAsJsonAsync("/api/contracts/1/status", updateDto);

            Assert.IsTrue(
                response.StatusCode == HttpStatusCode.OK ||
                response.StatusCode == HttpStatusCode.NotFound,
                $"Expected OK or NotFound, got {response.StatusCode}");
        }
    }
}
