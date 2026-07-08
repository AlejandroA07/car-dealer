namespace WestcoastCars.Web.Services;

public class VerificationResult
{
    public bool Succeeded { get; init; }
    public string? SessionToken { get; init; }
    public string? VerifiedEmailToken { get; init; }
    public string? ErrorMessage { get; init; }

    public static VerificationResult RequestSuccess(string sessionToken) => new()
    {
        Succeeded = true,
        SessionToken = sessionToken
    };

    public static VerificationResult ConfirmSuccess(string verifiedEmailToken) => new()
    {
        Succeeded = true,
        VerifiedEmailToken = verifiedEmailToken
    };

    public static VerificationResult Failure(string errorMessage) => new()
    {
        Succeeded = false,
        ErrorMessage = errorMessage
    };
}
