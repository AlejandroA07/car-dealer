using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WestcoastCars.Auth.Application.Services;
using WestcoastCars.Auth.Contracts.Admin;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace WestcoastCars.Auth.Api.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "Admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
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

                return Ok(new { Message = "User created successfully", UserId = authResult.User.Id });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}