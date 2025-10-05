using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using System;
using System.Diagnostics;
using WestcoastCars.Api.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace WestcoastCars.Api.Controllers
{
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ErrorController : ControllerBase
    {
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
                Instance = HttpContext.Request.Path,
                Detail = exception.Message
            };

            var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            problemDetails.Extensions["traceId"] = traceId;

            if (hostEnvironment.IsDevelopment())
            {
                problemDetails.Extensions["stackTrace"] = exception.StackTrace;
            }

            var logger = HttpContext.RequestServices.GetRequiredService<ILogger<ErrorController>>();
            logger.LogError(exception, "An error occurred with traceId {TraceId}: {ErrorMessage}", traceId, exception.Message);

            return StatusCode(problemDetails.Status.Value, problemDetails);
        }
    }
}
