using WestcoastCars.Application.Features.Vehicles.Commands.Create;
using WestcoastCars.Application.Features.Vehicles.Commands.Update;
using WestcoastCars.Application.Features.Vehicles.Validators;
using Xunit;

namespace WestcoastCars.Application.Tests.Features.Vehicles.Validators;

public class VehicleCommandValidatorTests
{
    [Theory]
    [InlineData(1899)]
    public void CreateVehicleCommandValidator_ShouldRejectInvalidModelYear(int modelYear)
    {
        var validator = new CreateVehicleCommandValidator();
        var command = CreateValidCreateCommand();
        command.ModelYear = modelYear;

        var result = validator.Validate(command);

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateVehicleCommand.ModelYear));
    }

    [Fact]
    public void CreateVehicleCommandValidator_ShouldRejectFutureModelYearBeyondNextYear()
    {
        var validator = new CreateVehicleCommandValidator();
        var command = CreateValidCreateCommand();
        command.ModelYear = DateTime.UtcNow.Year + 2;

        var result = validator.Validate(command);

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateVehicleCommand.ModelYear));
    }

    [Theory]
    [InlineData(1899)]
    public void UpdateVehicleCommandValidator_ShouldRejectInvalidModelYear(int modelYear)
    {
        var validator = new UpdateVehicleCommandValidator();
        var command = CreateValidUpdateCommand();
        command.ModelYear = modelYear;

        var result = validator.Validate(command);

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(UpdateVehicleCommand.ModelYear));
    }

    [Fact]
    public void UpdateVehicleCommandValidator_ShouldRejectFutureModelYearBeyondNextYear()
    {
        var validator = new UpdateVehicleCommandValidator();
        var command = CreateValidUpdateCommand();
        command.ModelYear = DateTime.UtcNow.Year + 2;

        var result = validator.Validate(command);

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(UpdateVehicleCommand.ModelYear));
    }

    private static CreateVehicleCommand CreateValidCreateCommand() =>
        new()
        {
            RegistrationNumber = "ABC123",
            ManufacturerId = 1,
            Model = "XC60",
            ModelYear = DateTime.UtcNow.Year,
            FuelTypeId = 1,
            TransmissionTypeId = 1,
            Price = 100000
        };

    private static UpdateVehicleCommand CreateValidUpdateCommand() =>
        new()
        {
            Id = 1,
            RegistrationNumber = "ABC123",
            ManufacturerId = 1,
            Model = "XC60",
            ModelYear = DateTime.UtcNow.Year,
            FuelTypeId = 1,
            TransmissionTypeId = 1,
            Price = 100000
        };
}
