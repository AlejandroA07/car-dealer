using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using westcoast_cars.web.Services;
using westcoast_cars.web.ViewModels.ServiceBooking;

namespace westcoast_cars.web.Controllers
{
    public class ServiceController : Controller
    {
        private readonly IServiceBookingService _bookingService;

        public ServiceController(IServiceBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(new ServiceBookingViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ServiceBookingViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var success = await _bookingService.CreateBookingAsync(model);

            if (success)
            {
                TempData["success"] = "Din bokning har mottagits! Vi kontaktar dig snart.";
                return RedirectToAction(nameof(Confirmation));
            }

            TempData["error"] = "Ett fel uppstod när bokningen skulle skickas. Försök igen senare.";
            return View(model);
        }

        [HttpGet]
        public IActionResult Confirmation()
        {
            return View();
        }

        [HttpGet("admin/bookings")]
        public async Task<IActionResult> AdminList()
        {
            if (!User.IsInRole("Admin") && !User.IsInRole("Salesperson"))
            {
                return Forbid();
            }

            var bookings = await _bookingService.ListAllBookingsAsync();
            return View(bookings);
        }
    }
}
