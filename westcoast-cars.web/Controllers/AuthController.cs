using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using westcoast_cars.web.Services;
using westcoast_cars.web.ViewModels.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Collections.Generic;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;

namespace westcoast_cars.web.Controllers
{
    [Route("auth")]
    public class AuthController : Controller
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpGet("login")]
        public IActionResult Login(string returnUrl = "/")
        {
            if (User.Identity.IsAuthenticated)
            {
                return LocalRedirect(returnUrl);
            }
            var model = new LoginViewModel { ReturnUrl = returnUrl };
            return View(model);
        }

        [HttpPost("login")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _authService.LoginAsync(model);

            if (result.IsSuccess)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, model.Email),
                    new Claim(ClaimTypes.NameIdentifier, model.Email)
                };

                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(result.Token);
                
                var roleClaims = jwtToken.Claims.Where(c => c.Type == "role" || c.Type == ClaimTypes.Role);
                foreach (var roleClaim in roleClaims)
                {
                    claims.Add(new Claim(ClaimTypes.Role, roleClaim.Value));
                }

                var claimsIdentity = new ClaimsIdentity(
                    claims, CookieAuthenticationDefaults.AuthenticationScheme);

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = model.RememberMe,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(1)
                };

                // Store the access token properly so GetTokenAsync can retrieve it
                authProperties.StoreTokens(new List<AuthenticationToken>
                {
                    new AuthenticationToken
                    {
                        Name = "access_token",
                        Value = result.Token
                    }
                });

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                TempData["success"] = "Login successful";
                return LocalRedirect(model.ReturnUrl ?? "/");
            }

            ModelState.AddModelError(string.Empty, result.Error);
            TempData["error"] = "Invalid login attempt";
            return View(model);
        }

        [HttpPost("logout")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            TempData["success"] = "You have been logged out";
            return RedirectToAction("Index", "Home");
        }
    }
}
