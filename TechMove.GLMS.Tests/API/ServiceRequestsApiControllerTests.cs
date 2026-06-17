using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.Net.Http.Json;
using TechMove.GLMS.API.DTOs;
using TechMove.GLMS.API.Models;
using TechMove.GLMS.API.Models.Enums;

namespace TechMove.GLMS.Tests.API
{
    [TestClass]
    public class ServiceRequestsApiControllerTests
    {
        private readonly ApiWebApplicationFactory _factory = new();

        // GET /api/servicerequests
        [TestMethod]
        public async Task GetAll_ReturnsSuccess()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("/api/servicerequests");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        // GET /api/servicerequests/1
        [TestMethod]
        public async Task GetById_ReturnsServiceRequest()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("/api/servicerequests/1");

            Assert.IsTrue(
                response.StatusCode == HttpStatusCode.OK ||
                response.StatusCode == HttpStatusCode.NotFound,
                $"Expected OK or NotFound, got {response.StatusCode}");

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var data = await response.Content.ReadFromJsonAsync<ServiceRequest>();

                Assert.IsNotNull(data);
                Assert.AreEqual(1, data.Id);
            }
        }

        // POST /api/servicerequests
        [TestMethod]
        public async Task Create_ReturnsCreatedOrBadRequest()
        {
            var client = _factory.CreateClient();

            var newRequest = new ServiceRequestCreateDto
            {
                ContractId = 1,
                Description = "API Test Request",
                CostUSD = 50
            };

            var response = await client.PostAsJsonAsync("/api/servicerequests", newRequest);

            Assert.IsTrue(
                response.StatusCode == HttpStatusCode.Created ||
                response.StatusCode == HttpStatusCode.BadRequest,
                $"Expected Created or BadRequest, got {response.StatusCode}");
        }

        // PUT /api/servicerequests/1
        [TestMethod]
        public async Task Update_ReturnsSuccessOrNotFound()
        {
            var client = _factory.CreateClient();

            var updateDto = new ServiceRequestUpdateDto
            {
                Description = "Updated API Request",
                Status = ServiceRequestStatus.InProgress,
                CostUSD = 75,
                CostZAR = 1400
            };

            var response = await client.PutAsJsonAsync("/api/servicerequests/1", updateDto);

            Assert.IsTrue(
                response.StatusCode == HttpStatusCode.OK ||
                response.StatusCode == HttpStatusCode.NotFound,
                $"Expected OK or NotFound, got {response.StatusCode}");
        }

        // DELETE /api/servicerequests/1
        [TestMethod]
        public async Task Delete_ReturnsSuccessOrNotFound()
        {
            var client = _factory.CreateClient();

            var response = await client.DeleteAsync("/api/servicerequests/1");

            Assert.IsTrue(
                response.StatusCode == HttpStatusCode.OK ||
                response.StatusCode == HttpStatusCode.NotFound,
                $"Expected OK or NotFound, got {response.StatusCode}");
        }
    }
}
