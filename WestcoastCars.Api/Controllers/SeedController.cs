using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WestcoastCars.Infrastructure.Data;

namespace WestcoastCars.Api.Controllers;

/// <summary>
/// Manual data seeding operations.
/// </summary>
[ApiController]
[Route("api/v1/seed")]
[Tags("Seed")]
public class SeedController(WestcoastCarsContext context, ILogger<SeedController> logger) : ControllerBase
{
    private readonly WestcoastCarsContext _context = context;
    private readonly ILogger<SeedController> _logger = logger;

    /// <summary>
    /// Seeds manufacturers, fuel types, transmissions, and vehicles from the bundled JSON file.
    /// Skips if vehicles already exist. Requires Admin role.
    /// </summary>
    [HttpPost("vehicles")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> SeedVehicles()
    {
        if (await SeedData.HasVehiclesAsync(_context))
        {
            _logger.LogInformation("Vehicle seed skipped — vehicles already exist.");
            return Ok(new { seeded = false, message = "Vehicles already exist. No action taken." });
        }

        await SeedData.LoadVehicleData(_context);

        _logger.LogInformation("Vehicle seed (with lookups) completed.");
        return Ok(new { seeded = true, message = "Manufacturers, fuel types, transmissions, and vehicles seeded successfully." });
    }
}
