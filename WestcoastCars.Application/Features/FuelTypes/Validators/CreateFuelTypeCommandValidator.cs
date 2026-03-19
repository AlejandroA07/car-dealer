using FluentValidation;
using WestcoastCars.Application.Features.FuelTypes.Commands.Create;

namespace WestcoastCars.Application.Features.FuelTypes.Validators
{
    public class CreateFuelTypeCommandValidator : AbstractValidator<CreateFuelTypeCommand>
    {
        public CreateFuelTypeCommandValidator()
        {
            RuleFor(ft => ft.Name)
                .NotEmpty().WithMessage("Fuel type name is required.")
                .MaximumLength(50).WithMessage("Fuel type name must not exceed 50 characters.");
        }
    }
}
