using FluentValidation;
using WestcoastCars.Application.Features.Manufacturers.Commands.Create;

namespace WestcoastCars.Application.Features.Manufacturers.Validators
{
    public class CreateManufacturerCommandValidator : AbstractValidator<CreateManufacturerCommand>
    {
        public CreateManufacturerCommandValidator()
        {
            RuleFor(m => m.Name)
                .NotEmpty().WithMessage("Manufacturer name is required.")
                .MaximumLength(50).WithMessage("Manufacturer name must not exceed 50 characters.");
        }
    }
}
