using System.Text.Json.Serialization;

namespace WestcoastCars.Application.Models.Blocket;

public class BlocketCarAdDetails
{
    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("subtitle")]
    public string? Subtitle { get; set; }

    [JsonPropertyName("model_year")]
    public string? ModelYearText { get; set; }

    [JsonPropertyName("mileage")]
    public string? Mileage { get; set; }

    [JsonPropertyName("transmission")]
    public string? Transmission { get; set; }

    [JsonPropertyName("fuel")]
    public string? Fuel { get; set; }

    [JsonPropertyName("price")]
    public string? Price { get; set; }

    [JsonPropertyName("specifications")]
    public Dictionary<string, string> Specifications { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    [JsonPropertyName("seller_type")]
    public string? SellerType { get; set; }

    [JsonPropertyName("ad_id")]
    public string? AdId { get; set; }

    [JsonPropertyName("image")]
    public BlocketImage? Image { get; set; }

    [JsonPropertyName("equipment")]
    public List<string> Equipment { get; set; } = [];
}
