using FluentValidation;
using WestcoastCars.Application.Features.ServiceBookings.Commands.Create;

namespace WestcoastCars.Application.Features.ServiceBookings.Validators;

public class CreateServiceBookingCommandValidator : AbstractValidator<CreateServiceBookingCommand>
{
    public CreateServiceBookingCommandValidator()
    {
        RuleFor(command => command.VehicleRegistrationNumber)
            .NotEmpty();

        RuleFor(command => command.ServiceType)
            .NotEmpty();

        RuleFor(command => command.CustomerName)
            .NotEmpty();

        RuleFor(command => command.CustomerEmail)
            .NotEmpty()
            .EmailAddress();

        RuleFor(command => command.CustomerPhone)
            .NotEmpty();
    }
}
