namespace WestcoastCars.Web.Configurations;

public class ImageUploadOptions
{
    public string StoragePath { get; set; } = "wwwroot/images/uploads";
    public string UrlPath { get; set; } = "/images/uploads";
    public long MaxFileSizeBytes { get; set; } = 5 * 1024 * 1024;
}
