using FluentValidation;
using WestcoastCars.Application.Features.ServiceBookings.Commands.Create;

namespace WestcoastCars.Application.Features.ServiceBookings.Validators;

public class CreateServiceBookingCommandValidator : AbstractValidator<CreateServiceBookingCommand>
{
    public CreateServiceBookingCommandValidator()
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
    }
}
