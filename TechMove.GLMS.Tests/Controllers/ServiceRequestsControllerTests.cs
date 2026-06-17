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
using TechMove.GLMS.Controllers;
using TechMove.GLMS.Helpers;
using TechMove.GLMS.API.Models;
using TechMove.GLMS.API.Models.Enums;
using TechMove.GLMS.Services;
using TechMove.GLMS.Tests;


namespace TechMove.GLMS.Tests.Controllers
{
    [TestClass]
    public class ServiceRequestsControllerTests
    {
        private ServiceRequestsController CreateController(
            HttpResponseMessage mainResponse,
            HttpResponseMessage? contractResponse = null,
            HttpResponseMessage? updateResponse = null)
        {
            var handlerMock = new Mock<HttpMessageHandler>();

            var queue = new Queue<HttpResponseMessage>();
            queue.Enqueue(mainResponse);
            if (contractResponse != null) queue.Enqueue(contractResponse);
            if (updateResponse != null) queue.Enqueue(updateResponse);

            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(() => queue.Dequeue());

            var httpClient = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("http://localhost")
            };
            httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            var factoryMock = new Mock<IHttpClientFactory>();
            factoryMock.Setup(f => f.CreateClient("API")).Returns(httpClient);

            var currencyMock = new Mock<CurrencyService>(null);
            currencyMock.Setup(c => c.ConvertUsdToZarAsync(It.IsAny<decimal>()))
                        .ReturnsAsync(1000m);

            var workflowMock = new Mock<WorkflowService>();
            workflowMock.Setup(w => w.CanCreateServiceRequest(It.IsAny<Contract>()))
                        .Returns(true);

            var controller = new ServiceRequestsController(
                factoryMock.Object,
                currencyMock.Object,
                workflowMock.Object);

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
            var list = new List<ServiceRequest>
            {
                new ServiceRequest { Id = 1, Description = "Test" }
            };

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(list))
            };

            var controller = CreateController(response);

            var result = await controller.Index(null, null);

            Assert.IsInstanceOfType(result, typeof(ViewResult));
        }

        [TestMethod]
        public async Task Index_ApiFails_ReturnsEmptyList()
        {
            var controller = CreateController(new HttpResponseMessage(HttpStatusCode.BadRequest));

            var result = await controller.Index(null, null);

            var view = (ViewResult)result;
            var model = (PaginatedList<ServiceRequest>)view.Model;

            Assert.AreEqual(0, model.Count);
        }

        // DETAILS
        [TestMethod]
        public async Task Details_ApiFails_ReturnsNotFound()
        {
            var controller = CreateController(new HttpResponseMessage(HttpStatusCode.NotFound));

            var result = await controller.Details(1);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task Details_ReturnsView()
        {
            var req = new ServiceRequest { Id = 1 };

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(req))
            };

            var controller = CreateController(response);

            var result = await controller.Details(1);

            Assert.IsInstanceOfType(result, typeof(ViewResult));
        }

        // CREATE
        [TestMethod]
        public async Task Create_InvalidModel_ReturnsView()
        {
            var controller = CreateController(new HttpResponseMessage(HttpStatusCode.OK));

            controller.ModelState.AddModelError("ContractId", "Required");

            var result = await controller.Create(new ServiceRequest());

            Assert.IsInstanceOfType(result, typeof(ViewResult));
        }

        [TestMethod]
        public async Task Create_ApiFails_ReturnsView()
        {
            var controller = CreateController(new HttpResponseMessage(HttpStatusCode.BadRequest));

            var result = await controller.Create(new ServiceRequest { ContractId = 1 });

            Assert.IsInstanceOfType(result, typeof(ViewResult));
        }

        [TestMethod]
        public async Task Create_ValidModel_RedirectsToIndex()
        {
            var controller = CreateController(new HttpResponseMessage(HttpStatusCode.OK));

            var result = await controller.Create(new ServiceRequest { ContractId = 1 });

            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
        }

        // EDIT
        [TestMethod]
        public async Task Edit_Get_ApiFails_ReturnsNotFound()
        {
            var controller = CreateController(new HttpResponseMessage(HttpStatusCode.NotFound));

            var result = await controller.Edit(1);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task Edit_Post_IdMismatch_ReturnsNotFound()
        {
            var controller = CreateController(new HttpResponseMessage(HttpStatusCode.OK));

            var result = await controller.Edit(1, new ServiceRequest { Id = 2 });

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task Edit_Post_ApiFails_ReturnsView()
        {
            var controller = CreateController(
                new HttpResponseMessage(HttpStatusCode.OK),
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(new Contract()))
                },
                new HttpResponseMessage(HttpStatusCode.BadRequest));

            var result = await controller.Edit(1, new ServiceRequest { Id = 1, ContractId = 1 });

            Assert.IsInstanceOfType(result, typeof(ViewResult));
        }

        [TestMethod]
        public async Task Edit_Post_ValidModel_RedirectsToIndex()
        {
            var controller = CreateController(
                new HttpResponseMessage(HttpStatusCode.OK),
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(new Contract()))
                },
                new HttpResponseMessage(HttpStatusCode.OK));

            var result = await controller.Edit(1, new ServiceRequest { Id = 1, ContractId = 1 });

            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
        }

        // DELETE
        [TestMethod]
        public async Task Delete_ApiFails_ReturnsNotFound()
        {
            var controller = CreateController(new HttpResponseMessage(HttpStatusCode.NotFound));

            var result = await controller.Delete(1);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task Delete_ReturnsView()
        {
            var req = new ServiceRequest { Id = 1 };

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(req))
            };

            var controller = CreateController(response);

            var result = await controller.Delete(1);

            Assert.IsInstanceOfType(result, typeof(ViewResult));
        }

        [TestMethod]
        public async Task DeleteConfirmed_RedirectsToIndex()
        {
            var controller = CreateController(new HttpResponseMessage(HttpStatusCode.OK));

            var result = await controller.DeleteConfirmed(1);

            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
        }
    }

    
}
