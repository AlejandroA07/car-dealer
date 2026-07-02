using WestcoastCars.Application.Features.Vehicles.Commands.BulkDelete;
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

    [Theory]
    [InlineData("/images/uploads/photo.jpg")]
    [InlineData("/images/no-car.png")]
    [InlineData("https://example.com/photo.jpg")]
    [InlineData("")]
    public void CreateVehicleCommandValidator_ShouldAcceptRelativeAndAbsoluteImageUrls(string imageUrl)
    {
        var validator = new CreateVehicleCommandValidator();
        var command = CreateValidCreateCommand();
        command.ImageUrl = imageUrl;

        var result = validator.Validate(command);

        Assert.DoesNotContain(result.Errors, error => error.PropertyName == nameof(CreateVehicleCommand.ImageUrl));
    }

    [Fact]
    public void CreateVehicleCommandValidator_ShouldRejectImageUrlWithoutLeadingSlashOrScheme()
    {
        var validator = new CreateVehicleCommandValidator();
        var command = CreateValidCreateCommand();
        command.ImageUrl = "not-a-valid-url";

        var result = validator.Validate(command);

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateVehicleCommand.ImageUrl));
    }

    [Theory]
    [InlineData("/images/uploads/photo.jpg")]
    [InlineData("https://example.com/photo.jpg")]
    [InlineData("")]
    public void UpdateVehicleCommandValidator_ShouldAcceptRelativeAndAbsoluteImageUrls(string imageUrl)
    {
        var validator = new UpdateVehicleCommandValidator();
        var command = CreateValidUpdateCommand();
        command.ImageUrl = imageUrl;

        var result = validator.Validate(command);

        Assert.DoesNotContain(result.Errors, error => error.PropertyName == nameof(UpdateVehicleCommand.ImageUrl));
    }

    [Fact]
    public void BulkDeleteVehiclesCommandValidator_ShouldFail_WhenNoFiltersProvided()
    {
        var validator = new BulkDeleteVehiclesCommandValidator();
        var result = validator.Validate(new BulkDeleteVehiclesCommand());
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Filters");
    }

    [Theory]
    [InlineData("Volvo", null, null, null, null)]
    [InlineData(null, "XC60", null, null, null)]
    [InlineData(null, null, true, null, null)]
    [InlineData(null, null, null, 0, null)]
    [InlineData(null, null, null, null, 50000)]
    public void BulkDeleteVehiclesCommandValidator_ShouldPass_WhenAtLeastOneFilterProvided(
        string? make, string? model, bool? isSold, int? minMileage, int? maxMileage)
    {
        var validator = new BulkDeleteVehiclesCommandValidator();
        var command = new BulkDeleteVehiclesCommand { Make = make, Model = model, IsSold = isSold, MinMileage = minMileage, MaxMileage = maxMileage };
        var result = validator.Validate(command);
        Assert.True(result.IsValid);
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
