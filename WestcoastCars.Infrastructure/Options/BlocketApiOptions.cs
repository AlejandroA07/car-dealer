namespace WestcoastCars.Infrastructure.Options;

public class BlocketApiOptions
{
    public const string SectionName = "BlocketApi";

    public string BaseUrl { get; set; } = "https://blocket-api.se/";
    public int TimeoutSeconds { get; set; } = 30;
    public string DefaultSortOrder { get; set; } = "PUBLISHED_DESC";
}
