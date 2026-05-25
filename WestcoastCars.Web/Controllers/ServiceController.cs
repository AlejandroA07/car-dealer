using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WestcoastCars.Web.Services;
using WestcoastCars.Web.ViewModels.ServiceBooking;

namespace WestcoastCars.Web.Controllers;

public class ServiceController : Controller
{
    private readonly IServiceBookingService _bookingService;

    public ServiceController(IServiceBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] DateOnly? weekStart)
    {
        var currentMonday = GetMonday(DateOnly.FromDateTime(DateTime.Today));
        var requestedWeek = weekStart ?? currentMonday;
        var monday = ClampWeekStart(GetMonday(requestedWeek), currentMonday, currentMonday.AddDays(42));
        var slotsResult = await _bookingService.GetWeekSlotsAsync(monday);
        return View(new ServiceIndexViewModel
        {
            BookingForm = new ServiceBookingViewModel { ServiceType = "Bas-service" },
            WeekStart = monday,
            WeekSlots = slotsResult.Data.ToList(),
            AvailabilityLoadFailed = !slotsResult.Succeeded,
            AvailabilityErrorMessage = slotsResult.ErrorMessage ?? string.Empty
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(ServiceIndexViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateWeekSlotsAsync(model);
            return View(model);
        }

        var result = await _bookingService.CreateBookingAsync(model.BookingForm);

        if (result.Succeeded)
        {
            TempData["success"] = "Din bokning är bekräftad. En bekräftelse har skickats till din e-post.";
            return RedirectToAction(nameof(Confirmation));
        }

        ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Ett fel uppstod när bokningen skulle skickas.");
        await PopulateWeekSlotsAsync(model);
        return View(model);
    }

    [HttpGet]
    public IActionResult Confirmation()
    {
        return View();
    }

    [HttpGet("admin/bookings")]
    [Authorize(Roles = "Admin,Salesperson")]
    public async Task<IActionResult> AdminList()
    {
        var result = await _bookingService.ListActiveBookingsAsync();
        if (!result.Succeeded)
            TempData["error"] = result.ErrorMessage ?? "Det gick inte att hämta bokningarna.";

        return View(new ServiceAdminListViewModel
        {
            Eyebrow = "Servicehantering",
            Title = "Aktiva bokningar",
            EmptyMessage = "Inga aktiva bokningar just nu.",
            IsHistoryView = false,
            Bookings = result.Data.ToList()
        });
    }

    [HttpGet("admin/bookings/history")]
    [Authorize(Roles = "Admin,Salesperson")]
    public async Task<IActionResult> AdminHistory()
    {
        var result = await _bookingService.ListInactiveBookingsAsync();
        if (!result.Succeeded)
            TempData["error"] = result.ErrorMessage ?? "Det gick inte att hämta bokningshistoriken.";

        return View("AdminList", new ServiceAdminListViewModel
        {
            Eyebrow = "Servicehistorik",
            Title = "Inaktiva bokningar",
            EmptyMessage = "Ingen bokningshistorik ännu.",
            IsHistoryView = true,
            Bookings = result.Data.ToList()
        });
    }

    [HttpPost("admin/bookings/{id}/cancel")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Salesperson")]
    public async Task<IActionResult> CancelBooking(int id, [FromForm] string cancellationReason)
    {
        var result = await _bookingService.CancelAsync(id, cancellationReason);
        TempData[result.Succeeded ? "success" : "error"] = result.Succeeded
            ? "Bokning avbokad."
            : result.ErrorMessage ?? "Det gick inte att avboka bokningen.";
        return RedirectToAction(nameof(AdminList));
    }

    [HttpPost("admin/bookings/{id}/complete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Salesperson")]
    public async Task<IActionResult> CompleteBooking(int id)
    {
        var result = await _bookingService.CompleteAsync(id);
        TempData[result.Succeeded ? "success" : "error"] = result.Succeeded
            ? "Bokning markerad som klar."
            : result.ErrorMessage ?? "Det gick inte att markera bokningen som klar.";
        return RedirectToAction(nameof(AdminList));
    }

    [HttpPost("admin/bookings/{id}/delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteBooking(int id)
    {
        var result = await _bookingService.DeleteAsync(id);
        TempData[result.Succeeded ? "success" : "error"] = result.Succeeded
            ? "Bokning raderad."
            : result.ErrorMessage ?? "Det gick inte att radera bokningen.";
        return RedirectToAction(nameof(AdminHistory));
    }

    // Same logic as ServiceBookingSchedule.GetMonday — duplicated here due to layer boundary.
    private static DateOnly GetMonday(DateOnly date)
    {
        var diff = ((int)date.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
        return date.AddDays(-diff);
    }

    private static DateOnly ClampWeekStart(DateOnly weekStart, DateOnly minWeekStart, DateOnly maxWeekStart)
    {
        if (weekStart < minWeekStart)
            return minWeekStart;

        if (weekStart > maxWeekStart)
            return maxWeekStart;

        return weekStart;
    }

    private async Task PopulateWeekSlotsAsync(ServiceIndexViewModel model)
    {
        var currentMonday = GetMonday(DateOnly.FromDateTime(DateTime.Today));
        model.WeekStart = ClampWeekStart(GetMonday(model.WeekStart), currentMonday, currentMonday.AddDays(42));
        var slotsResult = await _bookingService.GetWeekSlotsAsync(model.WeekStart);
        model.WeekSlots = slotsResult.Data.ToList();
        model.AvailabilityLoadFailed = !slotsResult.Succeeded;
        model.AvailabilityErrorMessage = slotsResult.ErrorMessage ?? string.Empty;
    }
}
