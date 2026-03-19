using FluentValidation;
using WestcoastCars.Application.Features.FuelTypes.Commands.Update;

namespace WestcoastCars.Application.Features.FuelTypes.Validators
{
    public class UpdateFuelTypeCommandValidator : AbstractValidator<UpdateFuelTypeCommand>
    {
        public UpdateFuelTypeCommandValidator()
        {
            RuleFor(ft => ft.Id)
                .GreaterThan(0).WithMessage("A valid fuel type ID is required.");

            RuleFor(ft => ft.Name)
                .NotEmpty().WithMessage("Fuel type name is required.")
                .MaximumLength(50).WithMessage("Fuel type name must not exceed 50 characters.");
        }
    }
}
