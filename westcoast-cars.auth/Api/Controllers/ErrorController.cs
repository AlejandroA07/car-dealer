
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using System;
using WestcoastCars.Auth.Api.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace WestcoastCars.Auth.Api.Controllers
{
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ErrorController : ControllerBase
    {
        private readonly ILogger<ErrorController> _logger;

        public ErrorController(ILogger<ErrorController> logger)
        {
            _logger = logger;
        }

        [Route("/error")]
        public IActionResult HandleError([FromServices] IHostEnvironment hostEnvironment)
        {
            var exceptionHandlerFeature = HttpContext.Features.Get<IExceptionHandlerFeature>()!;
            var exception = exceptionHandlerFeature.Error;

            var (statusCode, title) = exception switch
            {
                NotFoundException => (StatusCodes.Status404NotFound, "Not Found"),
                ConflictException => (StatusCodes.Status409Conflict, "Conflict"),
                _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.")
            };

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = exception.Message,
                Instance = HttpContext.Request.Path
            };

            var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            problemDetails.Extensions.Add("traceId", traceId);

            if (hostEnvironment.IsDevelopment())
            {
                problemDetails.Extensions.Add("stackTrace", exception.StackTrace);
            }

            _logger.LogError(exception, "An error occurred with traceId {TraceId}: {ErrorMessage}", traceId, exception.Message);

            return StatusCode(statusCode, problemDetails);
        }
    }
}

