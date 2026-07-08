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
            if (string.IsNullOrWhiteSpace(result.Token) || string.IsNullOrWhiteSpace(result.Email))
            {
                ModelState.AddModelError(string.Empty, "Ogiltigt svar från inloggningen.");
                TempData["error"] = "Inloggningen misslyckades";
                return View(model);
            }

            var identity = BuildIdentityFromJwt(result.Token, result.Email);
            if (identity is null)
            {
                ModelState.AddModelError(string.Empty, "Inloggningen misslyckades.");
                TempData["error"] = "Inloggningen misslyckades";
                return View(model);
            }

            await SignInAsync(identity, result.Token, model.RememberMe);

            TempData["success"] = "Du är inloggad";

            // Redirect Admins and Salespersons to the Dashboard if no specific ReturnUrl
            if (string.IsNullOrEmpty(model.ReturnUrl) || model.ReturnUrl == "/")
            {
                if (identity.HasClaim(c => c.Type == ClaimTypes.Role && (c.Value == "Admin" || c.Value == "Salesperson")))
                {
                    return RedirectToAction("Index", "Admin");
                }
            }

            return LocalRedirect(model.ReturnUrl ?? "/");
        }

        ModelState.AddModelError(string.Empty, result.Error ?? "Ogiltig e-post eller lösenord.");
        TempData["error"] = result.StatusCode == System.Net.HttpStatusCode.Forbidden
            ? "E-post ej bekräftad"
            : "Inloggningen misslyckades";
        return View(model);
    }

    [HttpGet("register")]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return LocalRedirect("/");
        }
        return View(new RegisterViewModel());
    }

    [HttpPost("register")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _authService.RegisterAsync(model);

        if (result.IsSuccess)
        {
            TempData["info"] = "Kontrollera din e-post för att bekräfta ditt konto innan du loggar in.";
            return RedirectToAction(nameof(Login));
        }

        ModelState.AddModelError(string.Empty, result.Error ?? "Registreringen misslyckades.");
        TempData["error"] = "Registreringen misslyckades";
        return View(model);
    }

    [HttpGet("confirm-email")]
    public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string token)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
        {
            TempData["error"] = "Bekräftelselänken saknar information.";
            return RedirectToAction(nameof(Login));
        }

        var result = await _authService.ConfirmEmailAsync(userId, token);

        if (!result.IsSuccess || string.IsNullOrWhiteSpace(result.Token) || string.IsNullOrWhiteSpace(result.Email))
        {
            TempData["error"] = result.Error ?? "Bekräftelselänken är ogiltig eller har gått ut.";
            return RedirectToAction(nameof(Login));
        }

        var identity = BuildIdentityFromJwt(result.Token, result.Email);
        if (identity is null)
        {
            TempData["error"] = "Bekräftelsen misslyckades.";
            return RedirectToAction(nameof(Login));
        }

        await SignInAsync(identity, result.Token, isPersistent: false);

        TempData["success"] = "Din e-post är bekräftad och du är nu inloggad.";
        return LocalRedirect("/");
    }

    [HttpPost("logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        TempData["success"] = "Du har loggats ut";
        return RedirectToAction("Index", "Home");
    }

    private ClaimsIdentity? BuildIdentityFromJwt(string token, string email)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, email),
            new(ClaimTypes.NameIdentifier, email)
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

        ClaimsPrincipal principal;
        try
        {
            principal = handler.ValidateToken(token, validationParams, out _);
        }
        catch (SecurityTokenException)
        {
            return null;
        }

        var roleClaims = principal.Claims.Where(c => c.Type == "role" || c.Type == ClaimTypes.Role);
        foreach (var roleClaim in roleClaims)
        {
            claims.Add(new Claim(ClaimTypes.Role, roleClaim.Value));
        }

        return new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    }

    private async Task SignInAsync(ClaimsIdentity identity, string token, bool isPersistent)
    {
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = isPersistent,
            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(1)
        };

        // Store the access token properly so GetTokenAsync can retrieve it
        authProperties.StoreTokens(
        [
            new AuthenticationToken
            {
                Name = "access_token",
                Value = token
            }
        ]);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            authProperties);
    }
}
