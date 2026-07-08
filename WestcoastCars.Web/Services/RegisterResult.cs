using System.Net;

namespace WestcoastCars.Web.Services;

public class RegisterResult
{
    public bool IsSuccess { get; }
    public HttpStatusCode? StatusCode { get; }
    public string? Error { get; }

    private RegisterResult(bool isSuccess, HttpStatusCode? statusCode = null, string? error = null)
    {
        IsSuccess = isSuccess;
        StatusCode = statusCode;
        Error = error;
    }

    public static RegisterResult Success() => new(true, HttpStatusCode.Accepted);
    public static RegisterResult Failure(HttpStatusCode? statusCode, string error) => new(false, statusCode, error);
}
