using FluentValidation;
using WestcoastCars.Application.Features.ServiceBookings.Commands.Create;
using WestcoastCars.Application.Interfaces;

namespace WestcoastCars.Application.Features.ServiceBookings.Validators;

public class CreateServiceBookingCommandValidator : AbstractValidator<CreateServiceBookingCommand>
{
    public CreateServiceBookingCommandValidator(IUnitOfWork unitOfWork)
    {
        RuleFor(command => command.VehicleRegistrationNumber)
            .NotEmpty()
            .MustAsync(async (registrationNumber, cancellationToken) =>
                await unitOfWork.VehicleRepository.FindByRegistrationNumberAsync(registrationNumber) is not null)
            .WithMessage("Vehicle with the specified registration number was not found.");

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
