using FluentValidation;
using WestcoastCars.Application.Features.Transmissions.Commands.Update;

namespace WestcoastCars.Application.Features.Transmissions.Validators
{
    public class UpdateTransmissionCommandValidator : AbstractValidator<UpdateTransmissionCommand>
    {
        public UpdateTransmissionCommandValidator()
        {
            RuleFor(t => t.Id)
                .GreaterThan(0).WithMessage("A valid transmission ID is required.");

            RuleFor(t => t.Name)
                .NotEmpty().WithMessage("Transmission name is required.")
                .MaximumLength(50).WithMessage("Transmission name must not exceed 50 characters.");
        }
    }
}
