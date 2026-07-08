using Microsoft.AspNetCore.Mvc;
using Moq;
using WestcoastCars.Api.Controllers;
using WestcoastCars.Application.Exceptions;
using WestcoastCars.Application.Services;
using WestcoastCars.Contracts.Verification;
using Xunit;

namespace WestcoastCars.Api.Tests;

public class ServiceBookingVerificationControllerTests
{
    private readonly Mock<IGuestEmailVerificationService> _verificationServiceMock = new();
    private readonly ServiceBookingVerificationController _controller;

    public ServiceBookingVerificationControllerTests()
    {
        _controller = new ServiceBookingVerificationController(_verificationServiceMock.Object);
    }

    [Fact]
    public async Task RequestCode_ShouldReturnAccepted_WithSessionToken()
    {
        _verificationServiceMock
            .Setup(s => s.RequestCodeAsync("guest@example.com", default))
            .ReturnsAsync("session-token");

        var result = await _controller.RequestCode(new RequestVerificationCodeDto { Email = "guest@example.com" });

        var acceptedResult = Assert.IsType<AcceptedResult>(result);
        var response = Assert.IsType<RequestVerificationCodeResponseDto>(acceptedResult.Value);
        Assert.Equal("session-token", response.SessionToken);
    }

    [Fact]
    public async Task RequestCode_ShouldPropagateException_WhenServiceFails()
    {
        var exception = new ValidationException("email", ["Invalid email."]);
        _verificationServiceMock
            .Setup(s => s.RequestCodeAsync(It.IsAny<string>(), default))
            .ThrowsAsync(exception);

        var actual = await Assert.ThrowsAsync<ValidationException>(
            () => _controller.RequestCode(new RequestVerificationCodeDto { Email = "guest@example.com" }));
        Assert.Same(exception, actual);
    }

    [Fact]
    public async Task ConfirmCode_ShouldReturnOk_WithVerifiedEmailToken()
    {
        _verificationServiceMock
            .Setup(s => s.ConfirmCodeAsync("session-token", "123456", default))
            .ReturnsAsync("verified-token");

        var result = await _controller.ConfirmCode(new ConfirmVerificationCodeDto { SessionToken = "session-token", Code = "123456" });

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ConfirmVerificationCodeResponseDto>(okResult.Value);
        Assert.Equal("verified-token", response.VerifiedEmailToken);
    }

    [Fact]
    public async Task ConfirmCode_ShouldPropagateException_WhenCodeIsWrong()
    {
        var exception = new ValidationException("code", ["The verification code is incorrect."]);
        _verificationServiceMock
            .Setup(s => s.ConfirmCodeAsync(It.IsAny<string>(), It.IsAny<string>(), default))
            .ThrowsAsync(exception);

        var actual = await Assert.ThrowsAsync<ValidationException>(
            () => _controller.ConfirmCode(new ConfirmVerificationCodeDto { SessionToken = "session-token", Code = "000000" }));
        Assert.Same(exception, actual);
    }
}
