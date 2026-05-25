using WestcoastCars.Contracts.DTOs;

namespace WestcoastCars.Web.ViewModels.ServiceBooking;

public class ServiceAdminListViewModel
{
    public string Eyebrow { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string EmptyMessage { get; set; } = string.Empty;
    public bool IsHistoryView { get; set; }
    public List<ServiceBookingSummaryDto> Bookings { get; set; } = [];
}
