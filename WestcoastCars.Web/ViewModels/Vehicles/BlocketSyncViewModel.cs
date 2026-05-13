using System.ComponentModel.DataAnnotations;

namespace WestcoastCars.Web.ViewModels.Vehicles;

public class BlocketSyncViewModel
{
    [Range(1, 50, ErrorMessage = "Limit must be between 1 and 50.")]
    public int Limit { get; set; } = 50;

    [Display(Name = "Store Id")]
    public string? OrgId { get; set; }

    [Display(Name = "Location")]
    public string? Locations { get; set; }

    [Display(Name = "Tillverkare")]
    public string? Manufacturers { get; set; }

    public string? MileageBand { get; set; }
    public string? TransmissionFilter { get; set; }
    public string? FuelTypeFilter { get; set; }

    [Range(1900, 2100, ErrorMessage = "Year must be between 1900 and 2100.")]
    public int? YearFrom { get; set; }

    [Range(1900, 2100, ErrorMessage = "Year must be between 1900 and 2100.")]
    public int? YearTo { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Price must be a positive number.")]
    public int? PriceFrom { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Price must be a positive number.")]
    public int? PriceTo { get; set; }

    public int RequestedLimit { get; set; }
    public int AppliedLimit { get; set; }
    public int PagesFetched { get; set; }
    public int TotalFetched { get; set; }
    public int TotalPrepared { get; set; }
    public int TotalAdded { get; set; }
    public int TotalUpdated { get; set; }
    public int TotalFlagged { get; set; }
    public int TotalSkipped { get; set; }

    public bool HasResult { get; set; }
    public string? InfoMessage { get; set; }
    public string? ErrorMessage { get; set; }
}
