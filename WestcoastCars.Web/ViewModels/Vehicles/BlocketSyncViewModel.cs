using System.ComponentModel.DataAnnotations;
using WestcoastCars.Contracts.DTOs;

namespace WestcoastCars.Web.ViewModels.Vehicles;

public class BlocketSyncViewModel
{
    [Range(1, 50, ErrorMessage = "Limit must be between 1 and 50.")]
    public int Limit { get; set; } = 50;

    public string? Query { get; set; }
    public string? SortOrder { get; set; }

    [Display(Name = "Store Id")]
    public string? OrgId { get; set; }

    [Display(Name = "Location")]
    public string? Locations { get; set; }

    [Display(Name = "Tillverkare")]
    public string? Manufacturers { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Price must be a positive number.")]
    public int? PriceFrom { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Price must be a positive number.")]
    public int? PriceTo { get; set; }

    [Range(1900, 2100, ErrorMessage = "Year must be between 1900 and 2100.")]
    public int? YearFrom { get; set; }

    [Range(1900, 2100, ErrorMessage = "Year must be between 1900 and 2100.")]
    public int? YearTo { get; set; }

    public string? MileageBand { get; set; }
    public string? Colors { get; set; }
    public string? TransmissionFilter { get; set; }
    public string? WheelDrive { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Horsepower must be a positive number.")]
    public int? HorsepowerFrom { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Horsepower must be a positive number.")]
    public int? HorsepowerTo { get; set; }

    public string? FuelTypeFilter { get; set; }

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

    public List<BlocketPreviewDto> PreviewResults { get; set; } = [];
    public bool HasPreview { get; set; }
    public ImportSelectedResult? ImportResult { get; set; }
}
