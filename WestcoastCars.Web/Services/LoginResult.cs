namespace WestcoastCars.Web.Services;

public class LoginResult
{
    public bool IsSuccess { get; }
    public string? Token { get; }
    public string? Error { get; }

    private LoginResult(bool isSuccess, string? token = null, string? error = null)
    {
        IsSuccess = isSuccess;
        Token = token;
        Error = error;
    }

    public static LoginResult Success(string token) => new(true, token: token);
    public static LoginResult Failure(string error) => new(false, error: error);
}
