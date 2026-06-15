using FluentValidation;
using WestcoastCars.Application.Features.Vehicles.Commands.BulkDelete;

namespace WestcoastCars.Application.Features.Vehicles.Validators;

public class BulkDeleteVehiclesCommandValidator : AbstractValidator<BulkDeleteVehiclesCommand>
{
    public BulkDeleteVehiclesCommandValidator()
    {
        RuleFor(c => c)
            .Must(c => c.Make != null || c.Model != null || c.IsSold != null || c.MinMileage != null || c.MaxMileage != null)
            .WithName("Filters")
            .WithMessage("At least one filter must be specified.");
    }
}
