using System.Net;

namespace WestcoastCars.Web.Services;

public class LoginResult
{
    public bool IsSuccess { get; }
    public HttpStatusCode? StatusCode { get; }
    public string? Token { get; }
    public string? Email { get; }
    public string? Error { get; }

    private LoginResult(bool isSuccess, HttpStatusCode? statusCode = null, string? token = null, string? email = null, string? error = null)
    {
        IsSuccess = isSuccess;
        StatusCode = statusCode;
        Token = token;
        Email = email;
        Error = error;
    }

    public static LoginResult Success(string token, string email) => new(true, HttpStatusCode.OK, token: token, email: email);
    public static LoginResult Failure(string error, HttpStatusCode? statusCode = null) => new(false, statusCode, error: error);
}
