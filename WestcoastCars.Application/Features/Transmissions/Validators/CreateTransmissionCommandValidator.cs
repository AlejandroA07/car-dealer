using FluentValidation;
using WestcoastCars.Application.Features.Transmissions.Commands.Create;

namespace WestcoastCars.Application.Features.Transmissions.Validators
{
    public class CreateTransmissionCommandValidator : AbstractValidator<CreateTransmissionCommand>
    {
        public CreateTransmissionCommandValidator()
        {
            RuleFor(t => t.Name)
                .NotEmpty().WithMessage("Transmission name is required.")
                .MaximumLength(50).WithMessage("Transmission name must not exceed 50 characters.");
        }
    }
}
