
using WestcoastCars.Auth.Application.Services;
using WestcoastCars.Auth.Contracts.Admin;

namespace WestcoastCars.Auth.Api.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        [HttpPost("create-user")]
        public async Task<IActionResult> CreateUser(CreateUserRequest request)
        {
            try
            {
                var authResult = await _adminService.CreateUserAsync(
                    request.FirstName,
                    request.LastName,
                    request.Email,
                    request.Password,
                    request.Role
                );

                // In a real application, you might return a more specific DTO or just a success message
                return Ok(new { Message = "User created successfully", UserId = authResult.User.Id });
            }
            catch (Exception ex)
            {
                // In a real application, you would handle specific exceptions and return appropriate status codes
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
