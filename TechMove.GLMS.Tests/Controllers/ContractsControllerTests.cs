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
using TechMove.GLMS.API.Models;          // ✔ Contract model
using TechMove.GLMS.API.Models.Enums;    // ✔ ContractStatus
using TechMove.GLMS.Services;        // ✔ Correct MVC Services
using TechMove.GLMS.Controllers;          // ✔ Correct MVC controller
using TechMove.GLMS.Helpers;             // ✔ PaginatedList

namespace TechMove.GLMS.Tests.Controllers
{
    [TestClass]
    public class ContractsControllerTests
    {
        private ContractsController CreateController(HttpResponseMessage fakeResponse)
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

            // Mock FileService
            var fileServiceMock = new Mock<FileService>(null);

            // Create controller
            var controller = new ContractsController(factoryMock.Object, fileServiceMock.Object);

            // Mock Session
            var httpContext = new DefaultHttpContext();
            httpContext.Session = new TestSession();
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            };

            return controller;
        }

        // INDEX
        [TestMethod]
        public async Task Index_ReturnsView()
        {
            var list = new List<Contract>
            {
                new Contract { Id = 1, ClientId = 10, Status = ContractStatus.Active }
            };

            var fakeResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(list))
            };

            var controller = CreateController(fakeResponse);

            var result = await controller.Index(null, null, null, null, null);

            Assert.IsInstanceOfType(result, typeof(ViewResult));
        }

        [TestMethod]
        public async Task Index_ApiFails_ReturnsEmptyList()
        {
            var fakeResponse = new HttpResponseMessage(HttpStatusCode.BadRequest);

            var controller = CreateController(fakeResponse);

            var result = await controller.Index(null, null, null, null, null);

            var view = (ViewResult)result;
            var model = (PaginatedList<Contract>)view.Model;

            Assert.AreEqual(0, model.Count);
        }

        // DETAILS
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
            var contract = new Contract { Id = 1, ClientId = 10 };

            var fakeResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(contract))
            };

            var controller = CreateController(fakeResponse);

            var result = await controller.Details(1);

            Assert.IsInstanceOfType(result, typeof(ViewResult));
        }

        // CREATE
        [TestMethod]
        public async Task Create_InvalidModel_ReturnsView()
        {
            var controller = CreateController(new HttpResponseMessage(HttpStatusCode.OK));

            controller.ModelState.AddModelError("ClientId", "Required");

            var result = await controller.Create(new Contract(), null);

            Assert.IsInstanceOfType(result, typeof(ViewResult));
        }

        [TestMethod]
        public async Task Create_ApiFails_ReturnsView()
        {
            var fakeResponse = new HttpResponseMessage(HttpStatusCode.BadRequest);

            var controller = CreateController(fakeResponse);

            var result = await controller.Create(new Contract { ClientId = 10 }, null);

            Assert.IsInstanceOfType(result, typeof(ViewResult));
        }

        [TestMethod]
        public async Task Create_ValidModel_RedirectsToIndex()
        {
            var fakeResponse = new HttpResponseMessage(HttpStatusCode.OK);

            var controller = CreateController(fakeResponse);

            var result = await controller.Create(new Contract { ClientId = 10 }, null);

            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
        }

        // EDIT
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

            var result = await controller.Edit(1, new Contract { Id = 2 }, null);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task Edit_Post_ApiFails_ReturnsView()
        {
            var fakeResponse = new HttpResponseMessage(HttpStatusCode.BadRequest);

            var controller = CreateController(fakeResponse);

            var result = await controller.Edit(1, new Contract { Id = 1 }, null);

            Assert.IsInstanceOfType(result, typeof(ViewResult));
        }

        [TestMethod]
        public async Task Edit_Post_ValidModel_RedirectsToIndex()
        {
            var fakeResponse = new HttpResponseMessage(HttpStatusCode.OK);

            var controller = CreateController(fakeResponse);

            var result = await controller.Edit(1, new Contract { Id = 1 }, null);

            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
        }

        // DELETE
        [TestMethod]
        public async Task Delete_ApiFails_ReturnsNotFound()
        {
            var fakeResponse = new HttpResponseMessage(HttpStatusCode.NotFound);

            var controller = CreateController(fakeResponse);

            var result = await controller.Delete(1);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task Delete_ReturnsView()
        {
            var contract = new Contract { Id = 1 };

            var fakeResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(contract))
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

        // DOWNLOAD
        [TestMethod]
        public void Download_FileMissing_ReturnsNotFound()
        {
            var controller = CreateController(new HttpResponseMessage(HttpStatusCode.OK));

            var result = controller.Download(null);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }
    }
}
