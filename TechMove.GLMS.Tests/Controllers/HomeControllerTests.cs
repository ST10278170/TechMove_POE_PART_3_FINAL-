using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using TechMove.GLMS.Controllers;
using TechMove.GLMS.API.Models;
using TechMove.GLMS.API.Models.Enums;
using TechMove.GLMS.Tests;


namespace TechMove.GLMS.Tests.Controllers
{
    [TestClass]
    public class HomeControllerTests
    {
        private HomeController CreateController(
            HttpResponseMessage clientsResponse,
            HttpResponseMessage contractsResponse,
            HttpResponseMessage requestsResponse)
        {
            var handlerMock = new Mock<HttpMessageHandler>();

            var queue = new Queue<HttpResponseMessage>();
            queue.Enqueue(clientsResponse);
            queue.Enqueue(contractsResponse);
            queue.Enqueue(requestsResponse);

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

            var loggerMock = new Mock<ILogger<HomeController>>();

            var controller = new HomeController(loggerMock.Object, factoryMock.Object);

            var httpContext = new DefaultHttpContext();
            httpContext.Session = new TestSession();
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            };

            return controller;
        }

        [TestMethod]
        public async Task Index_ReturnsView()
        {
            var clients = new List<Client> { new Client { Id = 1 } };
            var contracts = new List<Contract> { new Contract { Id = 1, Status = ContractStatus.Active } };
            var requests = new List<ServiceRequest> { new ServiceRequest { Id = 1, Status = ServiceRequestStatus.Pending } };

            var controller = CreateController(
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(clients))
                },
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(contracts))
                },
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(requests))
                });

            var result = await controller.Index();

            Assert.IsInstanceOfType(result, typeof(ViewResult));
        }

        [TestMethod]
        public async Task Index_ApiFails_StillReturnsView()
        {
            var controller = CreateController(
                new HttpResponseMessage(HttpStatusCode.BadRequest),
                new HttpResponseMessage(HttpStatusCode.BadRequest),
                new HttpResponseMessage(HttpStatusCode.BadRequest));

            var result = await controller.Index();

            Assert.IsInstanceOfType(result, typeof(ViewResult));
        }
    }

    
}
