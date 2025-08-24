using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using westcoast_cars.web.Data;
using westcoast_cars.web.Models;
using westcoast_cars.web.ViewModels.Manufacturer;

namespace westcoast_cars.web.Controllers
{
    [Route("[controller]")]
    public class ManufacturerController : Controller
    {
        
        private readonly WestcoastCarsContext _context;

        public ManufacturerController(WestcoastCarsContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Create(){

            var manufacturers = await CreateList();

            var model = new ManufacturerPostViewModel
            {
                Manufacturers = manufacturers
            };

            return View("Create", model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(ManufacturerPostViewModel model){

            if (!ModelState.IsValid)
            {
                model.Manufacturers = await CreateList();
                return View(model);
            }

            // Check if it  allready exist
            if(await _context.Manufacturers.SingleOrDefaultAsync(c => c.Name.ToUpper() == model.Name.ToUpper()) is not null){
                ModelState.AddModelError("Name", $"Tillverkare {model.Name} finns redan i systemet.");
                model.Manufacturers = await CreateList();
                return View(model);
            }

            // Save to db
            //1 Create a data model instance of ManufacturerModel
            var make = new ManufacturerModel
            {
                Name = model.Name.ToUpper()
            };

            //2 Add the new manufacturer to change tracking list
            await _context.Manufacturers.AddAsync(make);

            //3 Save the db changes
            if (await _context.SaveChangesAsync() > 0)
            {
                return RedirectToAction(nameof(Create));
            }

            // Something went wrong
            ModelState.AddModelError("Name", "Ett fel har inträffat");
            model.Manufacturers = await CreateList();
            return View();
        }

        private async Task<IList<ManufacturerListViewModel>> CreateList()
        {
            var manufacturers = await _context.Manufacturers
                .OrderBy(c => c.Name)
                .Select(m => new ManufacturerListViewModel
                {
                    Id = m.Id,
                    Name = m.Name
                })
                .ToListAsync();

            return manufacturers;
        }
    }
}