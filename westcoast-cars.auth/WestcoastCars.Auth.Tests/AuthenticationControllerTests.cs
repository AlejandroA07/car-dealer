
using Microsoft.AspNetCore.Mvc;
using Moq;
using WestcoastCars.Auth.Api.Controllers;
using WestcoastCars.Auth.Application.Services;
using WestcoastCars.Auth.Contracts.Auth;
using Xunit;
using System.Threading.Tasks;
using WestcoastCars.Auth.Application.Common.Interfaces.Authentication;
using WestcoastCars.Auth.Domain.Entities;
using System;

namespace WestcoastCars.Auth.Tests
{
    public class AuthenticationControllerTests
    {
        private readonly Mock<IAuthService> _authServiceMock;
        private readonly AuthenticationController _controller;

        public AuthenticationControllerTests()
        {
            _authServiceMock = new Mock<IAuthService>();
            _controller = new AuthenticationController(_authServiceMock.Object);
        }

        [Fact]
        public async Task Register_ShouldReturnOk_WhenRegistrationIsSuccessful()
        {
            // Arrange
            var request = new RegisterRequest("John", "Doe", "john.doe@example.com", "Password123!");
            var authResult = new AuthenticationResult(
                new User { Id = Guid.NewGuid(), FirstName = "John", LastName = "Doe", Email = "john.doe@example.com", PasswordHash = "hashedpassword" },
                "some-jwt-token"
            );
            _authServiceMock.Setup(s => s.RegisterAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(authResult);

            // Act
            var result = await _controller.Register(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<AuthenticationResponse>(okResult.Value);
            Assert.Equal(authResult.User.Email, response.Email);
            Assert.Equal(authResult.Token, response.Token);
        }

        [Fact]
        public async Task Register_ShouldReturnBadRequest_WhenRegistrationFails()
        {
            // Arrange
            var errorMessage = "Registration failed.";
            _authServiceMock.Setup(s => s.RegisterAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception(errorMessage));

            // Act
            var result = await _controller.Register(new RegisterRequest("John", "Doe", "john.doe@example.com", "Password123!"));

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var errorResponse = badRequestResult.Value;
            var messageProperty = errorResponse.GetType().GetProperty("message");
            Assert.NotNull(messageProperty);
            var messageValue = messageProperty.GetValue(errorResponse);
            Assert.Equal(errorMessage, messageValue);
        }

        [Fact]
        public async Task Login_ShouldReturnOk_WhenLoginIsSuccessful()
        {
            // Arrange
            var request = new LoginRequest("john.doe@example.com", "Password123!");
            var authResult = new AuthenticationResult(
                new User { Id = Guid.NewGuid(), FirstName = "John", LastName = "Doe", Email = "john.doe@example.com", PasswordHash = "hashedpassword" },
                "some-jwt-token"
            );
            _authServiceMock.Setup(s => s.LoginAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(authResult);

            // Act
            var result = await _controller.Login(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<AuthenticationResponse>(okResult.Value);
            Assert.Equal(authResult.User.Email, response.Email);
            Assert.Equal(authResult.Token, response.Token);
        }

        [Fact]
        public async Task Login_ShouldReturnBadRequest_WhenLoginFails()
        {
            // Arrange
            var errorMessage = "Invalid credentials.";
            _authServiceMock.Setup(s => s.LoginAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception(errorMessage));

            // Act
            var result = await _controller.Login(new LoginRequest("john.doe@example.com", "WrongPassword!"));

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var errorResponse = badRequestResult.Value;
            var messageProperty = errorResponse.GetType().GetProperty("message");
            Assert.NotNull(messageProperty);
            var messageValue = messageProperty.GetValue(errorResponse);
            Assert.Equal(errorMessage, messageValue);
        }
    }
}
