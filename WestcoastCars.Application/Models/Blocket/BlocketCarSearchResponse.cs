using System.Text.Json.Serialization;

namespace WestcoastCars.Application.Models.Blocket;

public class BlocketCarSearchResponse
{
    [JsonPropertyName("docs")]
    public List<BlocketCarSearchItem> Docs { get; set; } = [];
}

public class BlocketCarSearchItem
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("heading")]
    public string Heading { get; set; } = string.Empty;

    [JsonPropertyName("location")]
    public string? Location { get; set; }

    [JsonPropertyName("timestamp")]
    public long Timestamp { get; set; }

    [JsonPropertyName("canonical_url")]
    public string? CanonicalUrl { get; set; }

    [JsonPropertyName("image")]
    public string? ImageUrl { get; set; }

    [JsonPropertyName("price")]
    public BlocketPrice? Price { get; set; }

    [JsonPropertyName("org_id")]
    public string? OrgId { get; set; }

    [JsonPropertyName("organisation_name")]
    public string? OrganisationName { get; set; }

    [JsonPropertyName("year")]
    public int? Year { get; set; }

    [JsonPropertyName("mileage")]
    public int? Mileage { get; set; }

    [JsonPropertyName("mileage_unit")]
    public string? MileageUnit { get; set; }

    [JsonPropertyName("regno")]
    public string? RegistrationNumber { get; set; }

    [JsonPropertyName("make")]
    public string? Make { get; set; }

    [JsonPropertyName("model")]
    public string? Model { get; set; }

    [JsonPropertyName("model_specification")]
    public string? ModelSpecification { get; set; }

    [JsonPropertyName("transmission")]
    public string? Transmission { get; set; }

    [JsonPropertyName("fuel")]
    public string? Fuel { get; set; }
}

public class BlocketPrice
{
    [JsonPropertyName("amount")]
    public int? Amount { get; set; }

    [JsonPropertyName("currency_code")]
    public string? CurrencyCode { get; set; }

    [JsonPropertyName("price_unit")]
    public string? PriceUnit { get; set; }
}
