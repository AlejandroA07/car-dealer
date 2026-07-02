using FluentValidation;
using WestcoastCars.Application.Features.Vehicles.Commands.Create;

namespace WestcoastCars.Application.Features.Vehicles.Validators;

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

        var maxModelYear = DateTime.UtcNow.Year + 1;

        RuleFor(v => v.ModelYear)
            .InclusiveBetween(1900, maxModelYear)
            .WithMessage($"Model year must be between 1900 and {maxModelYear}.");

        RuleFor(v => v.Price)
            .GreaterThan(0).WithMessage("Vehicle price must be greater than zero.");

        RuleFor(v => v.ManufacturerId)
            .GreaterThan(0).WithMessage("A valid manufacturer is required.");

        RuleFor(v => v.FuelTypeId)
            .GreaterThan(0).WithMessage("A valid fuel type is required.");

        RuleFor(v => v.TransmissionTypeId)
            .GreaterThan(0).WithMessage("A valid transmission type is required.");

        RuleFor(v => v.ImageUrl)
            .MaximumLength(500)
            .Must(url => string.IsNullOrEmpty(url) || url.StartsWith('/') || Uri.TryCreate(url, UriKind.Absolute, out _))
            .WithMessage("ImageUrl must be a root-relative path or a valid absolute URL.");

        RuleFor(v => v.Description)
            .MaximumLength(4000).WithMessage("Description must not exceed 4000 characters.");
    }
}
