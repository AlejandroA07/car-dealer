using Xunit;

namespace WestcoastCars.Api.IntegrationTests;

public sealed class OptionalBlocketE2EFactAttribute : FactAttribute
{
    public OptionalBlocketE2EFactAttribute()
    {
        if (!string.Equals(Environment.GetEnvironmentVariable("RUN_BLOCKET_E2E"), "1", StringComparison.OrdinalIgnoreCase))
        {
            Skip = "Set RUN_BLOCKET_E2E=1 to run external Blocket E2E coverage.";
        }
    }
}
