using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TechMove.GLMS.API.Controllers;
using TechMove.GLMS.API.DTOs;
using TechMove.GLMS.API.Services;

namespace TechMove.GLMS.Tests.Controllers
{
    [TestClass]
    public class AuthControllerTests
    {
        private AuthController CreateController(Mock<IAuthService> mockService)
        {
            return new AuthController(mockService.Object);
        }

        [TestMethod]
        public async Task Login_InvalidCredentials_ReturnsUnauthorized()
        {
            var mockService = new Mock<IAuthService>();
            mockService
                .Setup(s => s.LoginAsync(It.IsAny<AuthRequestDto>()))
                .ReturnsAsync((AuthResponseDto?)null);

            var controller = CreateController(mockService);

            var result = await controller.Login(new AuthRequestDto
            {
                Username = "wrong",
                Password = "wrong"
            });

            Assert.IsInstanceOfType(result, typeof(UnauthorizedObjectResult));
        }

        [TestMethod]
        public async Task Login_ValidCredentials_ReturnsOk()
        {
            var mockService = new Mock<IAuthService>();
            mockService
                .Setup(s => s.LoginAsync(It.IsAny<AuthRequestDto>()))
                .ReturnsAsync(new AuthResponseDto
                {
                    Token = "jwt-token",
                    Username = "test",
                    Role = "User"
                });

            var controller = CreateController(mockService);

            var result = await controller.Login(new AuthRequestDto
            {
                Username = "test",
                Password = "123"
            });

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        [TestMethod]
        public async Task Register_UserAlreadyExists_ReturnsBadRequest()
        {
            var mockService = new Mock<IAuthService>();
            mockService
                .Setup(s => s.RegisterAsync(It.IsAny<RegisterDto>()))
                .ReturnsAsync(false);

            var controller = CreateController(mockService);

            var result = await controller.Register(new RegisterDto
            {
                Username = "existing",
                Password = "123",
                Role = "User"
            });

            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task Register_NewUser_ReturnsCreated()
        {
            var mockService = new Mock<IAuthService>();
            mockService
                .Setup(s => s.RegisterAsync(It.IsAny<RegisterDto>()))
                .ReturnsAsync(true);

            var controller = CreateController(mockService);

            var result = await controller.Register(new RegisterDto
            {
                Username = "new",
                Password = "123",
                Role = "User"
            });

            Assert.IsInstanceOfType(result, typeof(CreatedResult));
        }
    }
}
