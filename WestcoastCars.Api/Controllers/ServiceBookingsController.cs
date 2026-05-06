using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WestcoastCars.Contracts.DTOs;
using WestcoastCars.Application.Features.ServiceBookings.Commands.Create;
using WestcoastCars.Application.Features.ServiceBookings.Queries.ListAll;

namespace WestcoastCars.Api.Controllers
{
    /// <summary>
    /// Operations for managing car service bookings.
    /// </summary>
    [ApiController]
    [Route("api/v1/service-bookings")]
    [Tags("Service Bookings")]
    public class ServiceBookingsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ServiceBookingsController> _logger;

        public ServiceBookingsController(IMediator mediator, ILogger<ServiceBookingsController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Lists all service bookings. Requires Admin or Salesperson role.
        /// </summary>
        /// <returns>A collection of service bookings.</returns>
        [HttpGet]
        [Authorize(Roles = "Admin,Salesperson")]
        [ProducesResponseType(typeof(IEnumerable<ServiceBookingSummaryDto>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> ListAll()
        {
            _logger.LogInformation("Retrieving all service bookings");
            var result = await _mediator.Send(new ListServiceBookingsQuery());
            return Ok(result);
        }

        /// <summary>
        /// Creates a new service booking.
        /// </summary>
        /// <param name="command">Booking details.</param>
        /// <returns>The ID of the created booking.</returns>
        [HttpPost]
        [AllowAnonymous]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Create(CreateServiceBookingCommand command)
        {
            _logger.LogInformation("Creating new service booking for vehicle: {RegNo}", command.VehicleRegistrationNumber);
            var id = await _mediator.Send(command);
            return Ok(new { id = id });
        }
    }
}
