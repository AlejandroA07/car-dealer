using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WestcoastCars.Contracts.DTOs;
using WestcoastCars.Application.Features.ServiceBookings.Commands.Create;
using WestcoastCars.Application.Features.ServiceBookings.Queries.ListAll;
using WestcoastCars.Api.Observability;
using System.Diagnostics;

namespace WestcoastCars.Api.Controllers;

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
    private readonly AppTelemetry _telemetry;

    public ServiceBookingsController(IMediator mediator, ILogger<ServiceBookingsController> logger, AppTelemetry telemetry)
    {
        _mediator = mediator;
        _logger = logger;
        _telemetry = telemetry;
    }

    /// <summary>
    /// Lists all service bookings. Requires Admin or Salesperson role.
    /// </summary>
    /// <returns>A collection of service bookings.</returns>
    /// <response code="200">Service bookings returned successfully.</response>
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
    /// <param name="dto">Booking details.</param>
    /// <returns>The ID of the created booking.</returns>
    /// <response code="200">Service booking created successfully.</response>
    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(typeof(CreateServiceBookingResponseDto), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create(ServiceBookingPostDto dto)
    {
        var command = new CreateServiceBookingCommand
        {
            VehicleRegistrationNumber = dto.VehicleRegistrationNumber,
            ServiceType = dto.ServiceType,
            BookingDate = dto.BookingDate,
            CustomerName = dto.CustomerName,
            CustomerEmail = dto.CustomerEmail,
            CustomerPhone = dto.CustomerPhone,
            Description = dto.Description
        };

        _logger.LogInformation("Creating new service booking for vehicle: {RegNo}", command.VehicleRegistrationNumber);

        using var activity = _telemetry.StartServiceBookingActivity(command.VehicleRegistrationNumber);
        var startedAt = Stopwatch.StartNew();

        try
        {
            var id = await _mediator.Send(command);
            startedAt.Stop();
            _telemetry.RecordServiceBookingOperation("success", startedAt.Elapsed);
            activity?.SetTag("service_booking.id", id);
            return Ok(new CreateServiceBookingResponseDto { Id = id });
        }
        catch
        {
            startedAt.Stop();
            _telemetry.RecordServiceBookingOperation("failure", startedAt.Elapsed);
            throw;
        }
    }
}
