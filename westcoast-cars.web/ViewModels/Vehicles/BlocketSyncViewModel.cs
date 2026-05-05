using System.ComponentModel.DataAnnotations;

namespace westcoast_cars.web.ViewModels.Vehicles;

public class BlocketSyncViewModel
{
    [Range(1, 50, ErrorMessage = "Limit must be between 1 and 50.")]
    public int Limit { get; set; } = 50;

    [Display(Name = "Store Id")]
    public string? OrgId { get; set; }

    [Display(Name = "Location")]
    public string? Locations { get; set; }

    [Display(Name = "Brand")]
    public string? Models { get; set; }

    public int RequestedLimit { get; set; }
    public int AppliedLimit { get; set; }
    public int PagesFetched { get; set; }
    public int TotalFetched { get; set; }
    public int TotalPrepared { get; set; }
    public int TotalImported { get; set; }
    public int TotalReplaced { get; set; }
    public int TotalSkipped { get; set; }

    public bool HasResult { get; set; }
    public string? InfoMessage { get; set; }
    public string? ErrorMessage { get; set; }
}
