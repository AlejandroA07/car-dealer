using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WestcoastCars.Auth.Application.Services;
using WestcoastCars.Auth.Contracts.Admin;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace WestcoastCars.Auth.Api.Controllers
{
    /// <summary>
    /// Administrative operations for user management. Requires Admin role.
    /// </summary>
    [ApiController]
    [Route("api/admin")]
    [Tags("Administration")]
    [Authorize(Roles = "Admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        /// <summary>
        /// Creates a new user with a specific role.
        /// </summary>
        /// <param name="request">User details and role.</param>
        /// <returns>Success message and user ID.</returns>
        /// <response code="200">User created successfully.</response>
        /// <response code="400">Invalid data or user already exists.</response>
        /// <response code="401">Unauthorized.</response>
        /// <response code="403">Forbidden (requires Admin role).</response>
        [HttpPost("create-user")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
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