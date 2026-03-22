using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WestcoastCars.Contracts.DTOs;
using WestcoastCars.Application.Features.ServiceBookings.Commands.Create;
using WestcoastCars.Application.Features.ServiceBookings.Queries.ListAll;

namespace WestcoastCars.Api.Controllers
{
    [ApiController]
    [Route("api/v1/service-bookings")]
    public class ServiceBookingsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ServiceBookingsController> _logger;

        public ServiceBookingsController(IMediator mediator, ILogger<ServiceBookingsController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Salesperson")]
        public async Task<IActionResult> ListAll()
        {
            _logger.LogInformation("Retrieving all service bookings");
            var result = await _mediator.Send(new ListServiceBookingsQuery());
            return Ok(result);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Create(CreateServiceBookingCommand command)
        {
            _logger.LogInformation("Creating new service booking for vehicle: {RegNo}", command.VehicleRegistrationNumber);
            var id = await _mediator.Send(command);
            return Ok(new { id = id });
        }
    }
}
