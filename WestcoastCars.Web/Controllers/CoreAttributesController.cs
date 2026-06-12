using Microsoft.AspNetCore.Mvc;
using WestcoastCars.Web.Services;
using WestcoastCars.Web.ViewModels.CoreAttributes;

namespace WestcoastCars.Web.Controllers;

[Route("CoreAttributes")]
public class CoreAttributesController(
    IManufacturerService manufacturerService,
    IFuelTypeService fuelTypeService,
    ITransmissionTypeService transmissionTypeService,
    ILogger<CoreAttributesController> logger) : Controller
{
    private record AttributeConfig(string Title, string Icon, string AddLabel, string ExistingLabel, string Placeholder);

    private static readonly IReadOnlyList<(string Type, AttributeConfig Config)> Configs =
    [
        ("manufacturers", new("Tillverkare",  "fa-solid fa-building",  "Lägg till tillverkare",  "Befintliga tillverkare",  "t.ex. Volvo")),
        ("fueltypes",     new("Bränsletyper", "fa-solid fa-gas-pump",  "Lägg till bränsletyp",   "Befintliga bränsletyper", "t.ex. Hybrid")),
        ("transmissions", new("Växellådor",   "fa-solid fa-gears",     "Lägg till växellåda",    "Befintliga växellådor",   "t.ex. Automat")),
    ];

    private readonly IManufacturerService _manufacturerService = manufacturerService;
    private readonly IFuelTypeService _fuelTypeService = fuelTypeService;
    private readonly ITransmissionTypeService _transmissionTypeService = transmissionTypeService;
    private readonly ILogger<CoreAttributesController> _logger = logger;

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var sections = new List<AttributeSectionViewModel>();
        foreach (var (type, config) in Configs)
        {
            sections.Add(new AttributeSectionViewModel
            {
                AttributeType = type,
                Title = config.Title,
                Icon = config.Icon,
                AddLabel = config.AddLabel,
                ExistingLabel = config.ExistingLabel,
                Placeholder = config.Placeholder,
                Items = await ListAllAsync(type),
            });
        }
        return View("Index", sections);
    }

    [HttpPost("{type}")]
    public async Task<IActionResult> Create(string type, AttributePostViewModel model)
    {
        var config = Configs.FirstOrDefault(c => string.Equals(c.Type, type, StringComparison.OrdinalIgnoreCase));
        if (config == default)
            return NotFound();

        if (!ModelState.IsValid)
        {
            TempData["error"] = "Namn måste anges";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            var result = await CreateAsync(type, model);
            TempData[result ? "success" : "error"] = result ? $"{config.Config.Title} sparades" : "API-fel: Kunde inte spara";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating core attribute of type {Type}", type);
            TempData["error"] = "Ett oväntat fel uppstod";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet("{type}/Delete/{id:int}")]
    public async Task<IActionResult> Delete(string type, int id)
    {
        if (!Configs.Any(c => string.Equals(c.Type, type, StringComparison.OrdinalIgnoreCase)))
            return NotFound();

        try
        {
            var result = await DeleteAsync(type, id);
            TempData[result ? "success" : "error"] = result ? "Raderad" : "Kunde inte radera";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting core attribute type {Type} id {Id}", type, id);
            TempData["error"] = "Ett oväntat fel uppstod";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task<IList<AttributeItemViewModel>> ListAllAsync(string type) =>
        type.ToLowerInvariant() switch
        {
            "manufacturers" => await _manufacturerService.ListAllAsync(),
            "fueltypes" => await _fuelTypeService.ListAllAsync(),
            "transmissions" => await _transmissionTypeService.ListAllAsync(),
            _ => throw new ArgumentException($"Unknown attribute type: {type}")
        };

    private async Task<bool> CreateAsync(string type, AttributePostViewModel model) =>
        type.ToLowerInvariant() switch
        {
            "manufacturers" => await _manufacturerService.CreateAsync(model),
            "fueltypes" => await _fuelTypeService.CreateAsync(model),
            "transmissions" => await _transmissionTypeService.CreateAsync(model),
            _ => throw new ArgumentException($"Unknown attribute type: {type}")
        };

    private async Task<bool> DeleteAsync(string type, int id) =>
        type.ToLowerInvariant() switch
        {
            "manufacturers" => await _manufacturerService.DeleteAsync(id),
            "fueltypes" => await _fuelTypeService.DeleteAsync(id),
            "transmissions" => await _transmissionTypeService.DeleteAsync(id),
            _ => throw new ArgumentException($"Unknown attribute type: {type}")
        };
}
