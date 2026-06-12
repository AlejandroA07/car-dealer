using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using WestcoastCars.Web.Services;
using WestcoastCars.Web.ViewModels.Auth;
using WestcoastCars.Web.Configurations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Collections.Generic;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;

namespace WestcoastCars.Web.Controllers;

[Route("auth")]
public class AuthController(IAuthService authService, IOptions<JwtSettings> jwtOptions) : Controller
{
    private readonly IAuthService _authService = authService;
    private readonly JwtSettings _jwtSettings = jwtOptions.Value;

    [HttpGet("login")]
    public IActionResult Login(string returnUrl = "/")
    {
        if (User.Identity?.IsAuthenticated == true)
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
            if (string.IsNullOrWhiteSpace(result.Token))
            {
                ModelState.AddModelError(string.Empty, "Ogiltigt svar från inloggningen.");
                TempData["error"] = "Inloggningen misslyckades";
                return View(model);
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, model.Email),
                new(ClaimTypes.NameIdentifier, model.Email)
            };

            var handler = new JwtSecurityTokenHandler();
            var validationParams = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret)),
                ValidateIssuer = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = _jwtSettings.Audience,
                ValidateLifetime = true
            };

            ClaimsPrincipal? principal = null;
            try
            {
                principal = handler.ValidateToken(result.Token, validationParams, out _);
            }
            catch (SecurityTokenException)
            {
                ModelState.AddModelError(string.Empty, "Inloggningen misslyckades.");
                TempData["error"] = "Inloggningen misslyckades";
                return View(model);
            }

            var roleClaims = principal.Claims.Where(c => c.Type == "role" || c.Type == ClaimTypes.Role);
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
            authProperties.StoreTokens(
            [
                new AuthenticationToken
                {
                    Name = "access_token",
                    Value = result.Token
                }
            ]);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            TempData["success"] = "Du är inloggad";

            // Redirect Admins and Salespersons to the Dashboard if no specific ReturnUrl
            if (string.IsNullOrEmpty(model.ReturnUrl) || model.ReturnUrl == "/")
            {
                if (claimsIdentity.HasClaim(c => c.Type == ClaimTypes.Role && (c.Value == "Admin" || c.Value == "Salesperson")))
                {
                    return RedirectToAction("Index", "Admin");
                }
            }

            return LocalRedirect(model.ReturnUrl ?? "/");
        }

        ModelState.AddModelError(string.Empty, result.Error ?? "Ogiltig e-post eller lösenord.");
        TempData["error"] = "Inloggningen misslyckades";
        return View(model);
    }

    [HttpPost("logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        TempData["success"] = "Du har loggats ut";
        return RedirectToAction("Index", "Home");
    }
}
