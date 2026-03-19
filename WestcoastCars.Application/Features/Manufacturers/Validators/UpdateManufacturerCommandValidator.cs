using FluentValidation;
using WestcoastCars.Application.Features.Manufacturers.Commands.Update;

namespace WestcoastCars.Application.Features.Manufacturers.Validators
{
    public class UpdateManufacturerCommandValidator : AbstractValidator<UpdateManufacturerCommand>
    {
        public UpdateManufacturerCommandValidator()
        {
            RuleFor(m => m.Id)
                .GreaterThan(0).WithMessage("A valid manufacturer ID is required.");

            RuleFor(m => m.Name)
                .NotEmpty().WithMessage("Manufacturer name is required.")
                .MaximumLength(50).WithMessage("Manufacturer name must not exceed 50 characters.");
        }
    }
}
