using WestcoastCars.Application.Models.Blocket;

namespace WestcoastCars.Application.Features.Vehicles.Commands.RefreshInventoryFromBlocket;

public class RefreshInventoryFromBlocketResult
{
    public int RequestedLimit { get; set; }
    public int AppliedLimit { get; set; }
    public int PagesFetched { get; set; }
    public int TotalFetched { get; set; }
    public int TotalPrepared { get; set; }
    public int TotalImported { get; set; }
    public int TotalReplaced { get; set; }
    public int TotalSkipped { get; set; }
    public List<BlocketVehicleImportData> Vehicles { get; set; } = [];
}
