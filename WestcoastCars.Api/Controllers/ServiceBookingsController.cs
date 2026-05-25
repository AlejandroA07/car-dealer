using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using WestcoastCars.Contracts.DTOs;
using WestcoastCars.Application.Exceptions;
using WestcoastCars.Application.Features.ServiceBookings.Commands.Cancel;
using WestcoastCars.Application.Features.ServiceBookings.Commands.Complete;
using WestcoastCars.Application.Features.ServiceBookings.Commands.Create;
using WestcoastCars.Application.Features.ServiceBookings.Commands.Delete;
using WestcoastCars.Application.Features.ServiceBookings.Queries.ListAll;
using WestcoastCars.Application.Features.ServiceBookings.Queries.GetWeekSlots;
using WestcoastCars.Domain.Common.Enums;
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
    [ProducesResponseType(typeof(PagedResult<ServiceBookingSummaryDto>), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> ListAll([FromQuery] PagedQueryDto pagination, [FromQuery] string? state = null)
    {
        _logger.LogInformation("Retrieving all service bookings");
        var activeFilter = state?.Trim().ToLowerInvariant() switch
        {
            null or "" or "all" => null,
            "active" => true,
            "inactive" or "history" => false,
            _ => throw new ValidationException(nameof(state), ["State must be one of: all, active, inactive."])
        };

        var result = await _mediator.Send(new ListServiceBookingsQuery
        {
            Page = pagination.Page,
            PageSize = pagination.PageSize,
            IsActive = activeFilter
        });
        return Ok(result);
    }

    /// <summary>
    /// Cancels a service booking. Requires Admin or Salesperson role.
    /// </summary>
    [HttpPatch("{id}/cancel")]
    [Authorize(Roles = "Admin,Salesperson")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> Cancel(int id, [FromBody] CancelServiceBookingDto dto)
    {
        _logger.LogInformation("Cancelling service booking {Id}", id);
        await _mediator.Send(new CancelServiceBookingCommand { Id = id, CancellationReason = dto.CancellationReason });
        return NoContent();
    }

    /// <summary>
    /// Returns slot availability for a given week (Mon–Fri, 3 slots/day).
    /// </summary>
    /// <param name="weekStart">Monday of the target week (YYYY-MM-DD).</param>
    /// <returns>15 slots with booked/free status.</returns>
    /// <response code="200">Slot availability returned.</response>
    [HttpGet("availability")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<SlotAvailabilityDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> GetAvailability([FromQuery] DateOnly weekStart)
    {
        var slots = await _mediator.Send(new GetWeekSlotsQuery { WeekStart = weekStart });
        return Ok(slots);
    }

    /// <summary>
    /// Marks a confirmed service booking as completed. Requires Admin or Salesperson role.
    /// </summary>
    [HttpPatch("{id}/complete")]
    [Authorize(Roles = "Admin,Salesperson")]
    [ProducesResponseType(204)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> Complete(int id)
    {
        _logger.LogInformation("Completing service booking {Id}", id);
        await _mediator.Send(new CompleteServiceBookingCommand { Id = id });
        return NoContent();
    }

    /// <summary>
    /// Permanently deletes a service booking. Requires Admin role.
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(204)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> Delete(int id)
    {
        _logger.LogInformation("Deleting service booking {Id}", id);
        await _mediator.Send(new DeleteServiceBookingCommand { Id = id });
        return NoContent();
    }

    /// <summary>
    /// Creates a new service booking.
    /// </summary>
    /// <param name="dto">Booking details.</param>
    /// <returns>The ID of the created booking.</returns>
    /// <response code="200">Service booking created successfully.</response>
    [HttpPost]
    [AllowAnonymous]
    [EnableRateLimiting("booking-create")]
    [ProducesResponseType(typeof(CreateServiceBookingResponseDto), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create(ServiceBookingPostDto dto)
    {
        var command = new CreateServiceBookingCommand
        {
            VehicleRegistrationNumber = dto.VehicleRegistrationNumber,
            ServiceType = dto.ServiceType,
            BookingDate = dto.BookingDate,
            TimeSlot = (TimeSlot)dto.TimeSlot,
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
