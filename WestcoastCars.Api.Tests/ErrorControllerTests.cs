using System;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using WestcoastCars.Api.Controllers;
using WestcoastCars.Application.Exceptions;
using Xunit;

namespace WestcoastCars.Api.Tests;

public class ErrorControllerTests
{
    [Fact]
    public void HandleError_ShouldExposeKnownExceptionDetail()
    {
        var loggerMock = new Mock<ILogger<ErrorController>>();
        var exception = new ConflictException("Email already exists");
        var controller = CreateController(exception, loggerMock);

        var result = controller.HandleError(new TestHostEnvironment(Environments.Production));

        var objectResult = Assert.IsType<ObjectResult>(result);
        var problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Equal(StatusCodes.Status409Conflict, objectResult.StatusCode);
        Assert.Equal("Conflict", problemDetails.Title);
        Assert.Equal("Email already exists", problemDetails.Detail);
        loggerMock.Verify(
            logger => logger.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, _) => state.ToString()!.Contains("An error occurred with traceId", StringComparison.Ordinal)),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void HandleError_ShouldMapNotFoundExceptionTo404()
    {
        var controller = CreateController(new NotFoundException("Manufacturer no longer exists."));

        var result = controller.HandleError(new TestHostEnvironment(Environments.Production));

        var objectResult = Assert.IsType<ObjectResult>(result);
        var problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Equal(StatusCodes.Status404NotFound, objectResult.StatusCode);
        Assert.Equal("Not Found", problemDetails.Title);
        Assert.Equal("Manufacturer no longer exists.", problemDetails.Detail);
    }

    private static readonly string[] expected = ["Password is too weak"];

    [Fact]
    public void HandleError_ShouldIncludeValidationErrors()
    {
        var exception = new ValidationException("Password", ["Password is too weak"]);
        var controller = CreateController(exception);

        var result = controller.HandleError(new TestHostEnvironment(Environments.Production));

        var objectResult = Assert.IsType<ObjectResult>(result);
        var problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
        var errors = Assert.IsType<IDictionary<string, string[]>>(problemDetails.Extensions["errors"], exactMatch: false);
        Assert.Equal(expected, errors["Password"]);
    }

    [Fact]
    public void HandleError_ShouldNotExposeUnknownExceptionMessage()
    {
        var controller = CreateController(new InvalidOperationException("secret database details"));

        var result = controller.HandleError(new TestHostEnvironment(Environments.Production));

        var objectResult = Assert.IsType<ObjectResult>(result);
        var problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        Assert.Equal("An unexpected error occurred.", problemDetails.Detail);
        Assert.DoesNotContain("secret", problemDetails.Detail);
        Assert.DoesNotContain("stackTrace", problemDetails.Extensions.Keys);
    }

    [Fact]
    public void HandleError_ShouldNotExposePersistenceExceptionMessage()
    {
        var controller = CreateController(new PersistenceException("Failed to create vehicle"));

        var result = controller.HandleError(new TestHostEnvironment(Environments.Production));

        var objectResult = Assert.IsType<ObjectResult>(result);
        var problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        Assert.Equal("A persistence error occurred.", problemDetails.Detail);
        Assert.DoesNotContain("Failed to create vehicle", problemDetails.Detail);
        Assert.DoesNotContain("stackTrace", problemDetails.Extensions.Keys);
    }

    [Fact]
    public void HandleError_ShouldIncludeStackTraceForPersistenceExceptionInDevelopment()
    {
        var controller = CreateController(new PersistenceException("Failed to create vehicle"));

        var result = controller.HandleError(new TestHostEnvironment(Environments.Development));

        var objectResult = Assert.IsType<ObjectResult>(result);
        var problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        Assert.Equal("A persistence error occurred.", problemDetails.Detail);
        Assert.Contains("stackTrace", problemDetails.Extensions.Keys);
    }

    [Fact]
    public void HandleError_ShouldIncludeStackTraceForUnknownExceptionInDevelopment()
    {
        var controller = CreateController(new InvalidOperationException("secret database details"));

        var result = controller.HandleError(new TestHostEnvironment(Environments.Development));

        var objectResult = Assert.IsType<ObjectResult>(result);
        var problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        Assert.Equal("An unexpected error occurred.", problemDetails.Detail);
        Assert.Contains("stackTrace", problemDetails.Extensions.Keys);
    }

    private static ErrorController CreateController(Exception exception, Mock<ILogger<ErrorController>>? loggerMock = null)
    {
        var services = new ServiceCollection();
        if (loggerMock is null)
        {
            services.AddLogging();
        }
        else
        {
            services.AddSingleton(loggerMock.Object);
        }

        var serviceProvider = services.BuildServiceProvider();

        var httpContext = new DefaultHttpContext
        {
            RequestServices = serviceProvider
        };
        httpContext.Request.Path = "/error-source";
        httpContext.Features.Set<IExceptionHandlerFeature>(new TestExceptionHandlerFeature(exception));

        return new ErrorController
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            }
        };
    }

    private sealed class TestExceptionHandlerFeature(Exception error) : IExceptionHandlerFeature
    {
        public Exception Error { get; } = error;
        public string Path { get; } = "/error-source";
        public Endpoint? Endpoint { get; }
        public RouteValueDictionary? RouteValues { get; } = [];
    }

    private sealed class TestHostEnvironment(string environmentName) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = environmentName;
        public string ApplicationName { get; set; } = "WestcoastCars.Api.Tests";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
