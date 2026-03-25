namespace WestcoastCars.Web.Configurations;

public class ServiceOptions
{
    public const string SectionName = "Services";
    public string ApiUrl { get; set; } = null!;
    public string AuthUrl { get; set; } = null!;
}
