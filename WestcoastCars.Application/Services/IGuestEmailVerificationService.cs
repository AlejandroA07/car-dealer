namespace WestcoastCars.Application.Services;

public interface IGuestEmailVerificationService
{
    /// <summary>
    /// Generates a one-time code, emails it to <paramref name="email"/>, and returns a short-lived
    /// session token the caller must echo back to <see cref="ConfirmCodeAsync"/>. This token travels
    /// as an ordinary request field — never as an Authorization: Bearer header.
    /// </summary>
    Task<string> RequestCodeAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates <paramref name="code"/> against the session token. Throws
    /// <see cref="Exceptions.ValidationException"/> if the token is malformed, expired, of the wrong
    /// purpose, or the code doesn't match. Returns a short-lived "verified email" token on success.
    /// </summary>
    Task<string> ConfirmCodeAsync(string sessionToken, string code, CancellationToken cancellationToken = default);

    /// <summary>
    /// Throws <see cref="Exceptions.ValidationException"/> unless <paramref name="verifiedEmailToken"/>
    /// is a valid, unexpired, correctly-purposed token whose email claim matches <paramref name="email"/>
    /// (case-insensitive). No-op on success.
    /// </summary>
    Task EnsureEmailIsVerifiedAsync(string? verifiedEmailToken, string email, CancellationToken cancellationToken = default);
}
