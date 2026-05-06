using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WestcoastCars.Api.Controllers;
using WestcoastCars.Application.Common.Interfaces.Authentication;
using WestcoastCars.Application.Services;
using WestcoastCars.Contracts.Admin;
using WestcoastCars.Contracts.Auth;
using WestcoastCars.Application.Models.Authentication;
using Xunit;

namespace WestcoastCars.Api.Tests;

public class AuthenticationControllerTests
{
    private readonly Mock<IAuthService> _authServiceMock = new();

    [Fact]
    public async Task Login_ShouldReturnOk_WhenLoginIsSuccessful()
    {
        var controller = new AuthenticationController(_authServiceMock.Object);
        var authResult = new AuthenticationResult(
            new AuthenticatedUser(Guid.NewGuid(), "John", "Doe", "john.doe@example.com"),
            "some-jwt-token"
        );

        _authServiceMock
            .Setup(service => service.LoginAsync("john.doe@example.com", "Password123!"))
            .ReturnsAsync(authResult);

        var result = await controller.Login(new LoginRequest("john.doe@example.com", "Password123!"));

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<AuthenticationResponse>(okResult.Value);
        Assert.Equal(authResult.User.Email, response.Email);
        Assert.Equal(authResult.Token, response.Token);
    }

    [Fact]
    public async Task Login_ShouldReturnUnauthorized_WhenCredentialsAreInvalid()
    {
        var controller = new AuthenticationController(_authServiceMock.Object);

        _authServiceMock
            .Setup(service => service.LoginAsync("john.doe@example.com", "WrongPassword!"))
            .ReturnsAsync((AuthenticationResult?)null);

        var result = await controller.Login(new LoginRequest("john.doe@example.com", "WrongPassword!"));

        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task Login_ShouldReturnBadRequest_WhenLoginThrows()
    {
        var controller = new AuthenticationController(_authServiceMock.Object);

        _authServiceMock
            .Setup(service => service.LoginAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception("Auth failed."));

        var result = await controller.Login(new LoginRequest("john.doe@example.com", "Password123!"));

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        AssertBadRequestMessage(badRequestResult, "Auth failed.");
    }

    [Fact]
    public async Task Register_ShouldReturnOk_WhenRegistrationIsSuccessful()
    {
        var controller = new AuthenticationController(_authServiceMock.Object);
        var authResult = new AuthenticationResult(
            new AuthenticatedUser(Guid.NewGuid(), "Jane", "Doe", "jane.doe@example.com"),
            "registered-jwt-token"
        );

        _authServiceMock
            .Setup(service => service.RegisterAsync("Jane", "Doe", "jane.doe@example.com", "Password123!"))
            .ReturnsAsync(authResult);

        var result = await controller.Register(new RegisterRequest("Jane", "Doe", "jane.doe@example.com", "Password123!"));

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<AuthenticationResponse>(okResult.Value);
        Assert.Equal(authResult.User.Id, response.Id);
        Assert.Equal(authResult.Token, response.Token);
    }

    [Fact]
    public async Task Register_ShouldReturnBadRequest_WhenRegistrationFails()
    {
        var controller = new AuthenticationController(_authServiceMock.Object);

        _authServiceMock
            .Setup(service => service.RegisterAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception("User already exists."));

        var result = await controller.Register(new RegisterRequest("Jane", "Doe", "jane.doe@example.com", "Password123!"));

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        AssertBadRequestMessage(badRequestResult, "User already exists.");
    }

    private static void AssertBadRequestMessage(BadRequestObjectResult badRequestResult, string expectedMessage)
    {
        Assert.NotNull(badRequestResult.Value);
        var messageProperty = badRequestResult.Value.GetType().GetProperty("message");
        Assert.NotNull(messageProperty);
        Assert.Equal(expectedMessage, messageProperty.GetValue(badRequestResult.Value));
    }
}

public class AdminControllerTests
{
    private readonly Mock<IAdminService> _adminServiceMock = new();

    [Fact]
    public void AdminController_ShouldRequireAdminRole()
    {
        var authorizeAttribute = typeof(AdminController)
            .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
            .Cast<AuthorizeAttribute>()
            .Single();

        Assert.Equal("Admin", authorizeAttribute.Roles);
    }

    [Fact]
    public async Task CreateUser_ShouldReturnOk_WhenUserIsCreated()
    {
        var controller = new AdminController(_adminServiceMock.Object);
        var authResult = new AuthenticationResult(
            new AuthenticatedUser(Guid.NewGuid(), "Sales", "User", "sales@example.com"),
            "created-user-jwt-token"
        );

        _adminServiceMock
            .Setup(service => service.CreateUserAsync("Sales", "User", "sales@example.com", "Password123!", "Salesperson"))
            .ReturnsAsync(authResult);

        var result = await controller.CreateUser(new CreateUserRequest("Sales", "User", "sales@example.com", "Password123!", "Salesperson"));

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task CreateUser_ShouldReturnBadRequest_WhenServiceFails()
    {
        var controller = new AdminController(_adminServiceMock.Object);

        _adminServiceMock
            .Setup(service => service.CreateUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception("Invalid role."));

        var result = await controller.CreateUser(new CreateUserRequest("Sales", "User", "sales@example.com", "Password123!", "Invalid"));

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        AssertBadRequestMessage(badRequestResult, "Invalid role.");
    }

    private static void AssertBadRequestMessage(BadRequestObjectResult badRequestResult, string expectedMessage)
    {
        Assert.NotNull(badRequestResult.Value);
        var messageProperty = badRequestResult.Value.GetType().GetProperty("message");
        Assert.NotNull(messageProperty);
        Assert.Equal(expectedMessage, messageProperty.GetValue(badRequestResult.Value));
    }
}
