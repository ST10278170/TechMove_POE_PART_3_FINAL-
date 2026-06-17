using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using TechMove.GLMS.Controllers;      // ✔ Correct MVC controller
using TechMove.GLMS.API.Models;       // ✔ Client model
using TechMove.GLMS.Helpers;          // ✔ PaginatedList

namespace TechMove.GLMS.Tests.Controllers
{
    [TestClass]
    public class ClientsControllerTests
    {
        private ClientsController CreateController(HttpResponseMessage fakeResponse)
        {
            // Mock HttpMessageHandler
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(fakeResponse);

            var httpClient = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("http://localhost")
            };
            httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            // Mock IHttpClientFactory
            var factoryMock = new Mock<IHttpClientFactory>();
            factoryMock
                .Setup(f => f.CreateClient("API"))
                .Returns(httpClient);

            // Create controller
            var controller = new ClientsController(factoryMock.Object);

            // Mock Session
            var httpContext = new DefaultHttpContext();
            httpContext.Session = new TestSession();
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            };

            return controller;
        }

        // -------------------------
        // INDEX TESTS
        // -------------------------

        [TestMethod]
        public async Task Index_ReturnsView_WithPaginatedList()
        {
            var clients = new List<Client>
            {
                new Client { Id = 1, Name = "Alpha", Region = "Cape Town" },
                new Client { Id = 2, Name = "Beta", Region = "Durban" }
            };

            var fakeResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(clients))
            };

            var controller = CreateController(fakeResponse);

            var result = await controller.Index(null, null);

            Assert.IsInstanceOfType(result, typeof(ViewResult));

            var view = (ViewResult)result;
            Assert.IsInstanceOfType(view.Model, typeof(PaginatedList<Client>));
        }

        [TestMethod]
        public async Task Index_ApiFails_ReturnsEmptyList()
        {
            var fakeResponse = new HttpResponseMessage(HttpStatusCode.BadRequest);

            var controller = CreateController(fakeResponse);

            var result = await controller.Index(null, null);

            var view = (ViewResult)result;
            var model = (PaginatedList<Client>)view.Model;

            Assert.AreEqual(0, model.Count);
        }

        [TestMethod]
        public async Task Index_SearchFiltersResults()
        {
            var clients = new List<Client>
            {
                new Client { Id = 1, Name = "Alpha" },
                new Client { Id = 2, Name = "Beta" }
            };

            var fakeResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(clients))
            };

            var controller = CreateController(fakeResponse);

            var result = await controller.Index(null, "Alpha");

            var view = (ViewResult)result;
            var model = (PaginatedList<Client>)view.Model;

            Assert.AreEqual(1, model.Count);
            Assert.AreEqual("Alpha", model[0].Name);
        }

        // -------------------------
        // DETAILS TESTS
        // -------------------------

        [TestMethod]
        public async Task Details_ApiFails_ReturnsNotFound()
        {
            var fakeResponse = new HttpResponseMessage(HttpStatusCode.NotFound);

            var controller = CreateController(fakeResponse);

            var result = await controller.Details(1);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task Details_ReturnsViewWithModel()
        {
            var client = new Client { Id = 1, Name = "Test" };

            var fakeResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(client))
            };

            var controller = CreateController(fakeResponse);

            var result = await controller.Details(1);

            var view = (ViewResult)result;
            Assert.IsInstanceOfType(view.Model, typeof(Client));
        }

        // -------------------------
        // CREATE TESTS
        // -------------------------

        [TestMethod]
        public async Task Create_InvalidModel_ReturnsView()
        {
            var controller = CreateController(new HttpResponseMessage(HttpStatusCode.OK));

            controller.ModelState.AddModelError("Name", "Required");

            var result = await controller.Create(new Client());

            Assert.IsInstanceOfType(result, typeof(ViewResult));
        }

        [TestMethod]
        public async Task Create_ApiFails_ReturnsViewWithError()
        {
            var fakeResponse = new HttpResponseMessage(HttpStatusCode.BadRequest);

            var controller = CreateController(fakeResponse);

            var result = await controller.Create(new Client { Name = "Test" });

            Assert.IsInstanceOfType(result, typeof(ViewResult));
            Assert.IsFalse(controller.ModelState.IsValid);
        }

        [TestMethod]
        public async Task Create_ValidModel_RedirectsToIndex()
        {
            var fakeResponse = new HttpResponseMessage(HttpStatusCode.OK);

            var controller = CreateController(fakeResponse);

            var result = await controller.Create(new Client { Name = "Test" });

            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
        }

        // -------------------------
        // EDIT TESTS
        // -------------------------

        [TestMethod]
        public async Task Edit_Get_ApiFails_ReturnsNotFound()
        {
            var fakeResponse = new HttpResponseMessage(HttpStatusCode.NotFound);

            var controller = CreateController(fakeResponse);

            var result = await controller.Edit(1);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task Edit_Post_IdMismatch_ReturnsNotFound()
        {
            var controller = CreateController(new HttpResponseMessage(HttpStatusCode.OK));

            var result = await controller.Edit(1, new Client { Id = 2 });

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task Edit_Post_ApiFails_ReturnsViewWithError()
        {
            var fakeResponse = new HttpResponseMessage(HttpStatusCode.BadRequest);

            var controller = CreateController(fakeResponse);

            var result = await controller.Edit(1, new Client { Id = 1 });

            Assert.IsInstanceOfType(result, typeof(ViewResult));
            Assert.IsFalse(controller.ModelState.IsValid);
        }

        [TestMethod]
        public async Task Edit_Post_ValidModel_RedirectsToIndex()
        {
            var fakeResponse = new HttpResponseMessage(HttpStatusCode.OK);

            var controller = CreateController(fakeResponse);

            var result = await controller.Edit(1, new Client { Id = 1 });

            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
        }

        // -------------------------
        // DELETE TESTS
        // -------------------------

        [TestMethod]
        public async Task Delete_ApiFails_ReturnsNotFound()
        {
            var fakeResponse = new HttpResponseMessage(HttpStatusCode.NotFound);

            var controller = CreateController(fakeResponse);

            var result = await controller.Delete(1);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task Delete_ReturnsViewWithModel()
        {
            var client = new Client { Id = 1, Name = "Test" };

            var fakeResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(client))
            };

            var controller = CreateController(fakeResponse);

            var result = await controller.Delete(1);

            Assert.IsInstanceOfType(result, typeof(ViewResult));
        }

        [TestMethod]
        public async Task DeleteConfirmed_RedirectsToIndex()
        {
            var fakeResponse = new HttpResponseMessage(HttpStatusCode.OK);

            var controller = CreateController(fakeResponse);

            var result = await controller.DeleteConfirmed(1);

            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
        }
    }
}
