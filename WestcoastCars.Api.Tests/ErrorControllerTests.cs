using System;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using WestcoastCars.Api.Controllers;
using WestcoastCars.Application.Exceptions;
using Xunit;

namespace WestcoastCars.Api.Tests;

public class ErrorControllerTests
{
    [Fact]
    public void HandleError_ShouldExposeKnownExceptionDetail()
    {
        var controller = CreateController(new ConflictException("Email already exists"));

        var result = controller.HandleError(new TestHostEnvironment(Environments.Production));

        var objectResult = Assert.IsType<ObjectResult>(result);
        var problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Equal(StatusCodes.Status409Conflict, objectResult.StatusCode);
        Assert.Equal("Conflict", problemDetails.Title);
        Assert.Equal("Email already exists", problemDetails.Detail);
    }

    [Fact]
    public void HandleError_ShouldIncludeValidationErrors()
    {
        var exception = new ValidationException("Password", new[] { "Password is too weak" });
        var controller = CreateController(exception);

        var result = controller.HandleError(new TestHostEnvironment(Environments.Production));

        var objectResult = Assert.IsType<ObjectResult>(result);
        var problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
        var errors = Assert.IsAssignableFrom<IDictionary<string, string[]>>(problemDetails.Extensions["errors"]);
        Assert.Equal(new[] { "Password is too weak" }, errors["Password"]);
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
    }

    private static ErrorController CreateController(Exception exception)
    {
        var services = new ServiceCollection()
            .AddLogging()
            .BuildServiceProvider();

        var httpContext = new DefaultHttpContext
        {
            RequestServices = services
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

    private sealed class TestExceptionHandlerFeature : IExceptionHandlerFeature
    {
        public TestExceptionHandlerFeature(Exception error)
        {
            Error = error;
        }

        public Exception Error { get; }
        public string Path { get; } = "/error-source";
        public Endpoint? Endpoint { get; }
        public RouteValueDictionary? RouteValues { get; } = new();
    }

    private sealed class TestHostEnvironment : IHostEnvironment
    {
        public TestHostEnvironment(string environmentName)
        {
            EnvironmentName = environmentName;
        }

        public string EnvironmentName { get; set; }
        public string ApplicationName { get; set; } = "WestcoastCars.Api.Tests";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
