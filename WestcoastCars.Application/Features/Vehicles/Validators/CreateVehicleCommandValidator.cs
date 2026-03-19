using FluentValidation;
using WestcoastCars.Application.Features.Vehicles.Commands.Create;

namespace WestcoastCars.Application.Features.Vehicles.Validators
{
    public class CreateVehicleCommandValidator : AbstractValidator<CreateVehicleCommand>
    {
        public CreateVehicleCommandValidator()
        {
            RuleFor(v => v.RegistrationNumber)
                .NotEmpty().WithMessage("Registration number is required.")
                .MaximumLength(10).WithMessage("Registration number must not exceed 10 characters.");

            RuleFor(v => v.Model)
                .NotEmpty().WithMessage("Model is required.")
                .MaximumLength(100).WithMessage("Model name must not exceed 100 characters.");

            RuleFor(v => v.ModelYear)
                .NotEmpty().WithMessage("Model year is required.")
                .Matches(@"^\d{4}$").WithMessage("Model year must be a 4-digit number.");

            RuleFor(v => v.Value)
                .GreaterThan(0).WithMessage("Vehicle value must be greater than zero.");

            RuleFor(v => v.ManufacturerId)
                .GreaterThan(0).WithMessage("A valid manufacturer is required.");

            RuleFor(v => v.FuelTypeId)
                .GreaterThan(0).WithMessage("A valid fuel type is required.");

            RuleFor(v => v.TransmissionTypeId)
                .GreaterThan(0).WithMessage("A valid transmission type is required.");
        }
    }
}
