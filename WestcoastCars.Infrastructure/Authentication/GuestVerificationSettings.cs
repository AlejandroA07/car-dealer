namespace WestcoastCars.Infrastructure.Authentication;

public class GuestVerificationSettings
{
    public const string SectionName = "GuestVerification";
    public string Secret { get; init; } = null!;
    public string Issuer { get; init; } = null!;
    public string Audience { get; init; } = null!;
    public int CodeLength { get; init; } = 6;
    public int CodeExpiryMinutes { get; init; } = 10;
    public int VerifiedTokenExpiryMinutes { get; init; } = 20;
}
