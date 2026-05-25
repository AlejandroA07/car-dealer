using FluentValidation;
using WestcoastCars.Application.Common.Interfaces.Services;
using WestcoastCars.Application.Features.ServiceBookings;
using WestcoastCars.Application.Features.ServiceBookings.Commands.Create;

namespace WestcoastCars.Application.Features.ServiceBookings.Validators;

public class CreateServiceBookingCommandValidator : AbstractValidator<CreateServiceBookingCommand>
{
    public CreateServiceBookingCommandValidator(IDateTimeProvider dateTimeProvider)
    {
        RuleFor(command => command.VehicleRegistrationNumber)
            .NotEmpty()
            .MaximumLength(10);

        RuleFor(command => command.ServiceType)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(command => command.CustomerName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(command => command.CustomerEmail)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256);

        RuleFor(command => command.CustomerPhone)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(command => command.Description)
            .MaximumLength(2000);

        RuleFor(command => command.TimeSlot)
            .IsInEnum()
            .WithMessage("Ogiltigt tidsfönster.");

        RuleFor(command => command)
            .Must(command => !ServiceBookingSchedule.HasSlotPassed(dateTimeProvider.LocalNow, command.BookingDate, command.TimeSlot))
            .WithMessage("Det valda tidsfönstret har redan passerat.");
    }
}
