# Email Verification on Account Registration

## Goal

Require users to verify their email before they can log in. No JWT is issued at registration — only after the email is confirmed.

---

## Current State

| Thing | Status |
|---|---|
| `UserManager<IdentityUser>` | Registered |
| `AddDefaultTokenProviders()` | Registered — token gen/validation works |
| `EmailService` (MailKit/SMTP) | Exists, works for bookings |
| `IEmailService` | Injectable interface |
| `SignIn.RequireConfirmedEmail` | **Not set** (defaults `false`) |
| `RegisterAsync` | Creates user + returns JWT immediately |
| `LoginAsync` | Does not check `IsNotAllowed` |

Identity already handles token generation and email confirmation natively — nothing needs to be installed.

---

## Target Flow

```
POST /api/auth/register
  → create user
  → GenerateEmailConfirmationTokenAsync
  → send email with confirmation link
  → return 202 Accepted  (no JWT)

User clicks link in email
  → GET /api/auth/confirm-email?userId=...&token=...
  → ConfirmEmailAsync
  → return 200 + JWT

POST /api/auth/login  (before confirming email)
  → CheckPasswordSignInAsync → IsNotAllowed = true
  → return 403 Forbidden  "Email not confirmed"

POST /api/auth/login  (after confirming)
  → normal flow → 200 + JWT
```

---

## Decisions

| Decision | Choice | Reason |
|---|---|---|
| Confirm endpoint verb | `GET` with query params | Email clients open links as GET; avoids asking users to copy tokens |
| Token encoding | `Uri.EscapeDataString` | Identity tokens contain `+`, `/`, `=` — must be URL-encoded |
| Confirmation link owner | API builds the link using `AppBaseUrl` config | Keeps frontend decoupled; single source of truth |
| Unconfirmed login response | `403 Forbidden` | Distinct from `401` (wrong password) — easier to handle on clients |
| Register response | `202 Accepted` + `{ message }` | No JWT until confirmed; signals async step is pending |

---

## Files

### New files

| File | What |
|---|---|
| `WestcoastCars.Contracts/Auth/ConfirmEmailRequest.cs` | Record with `UserId` + `Token` (query params) |
| `WestcoastCars.Contracts/Auth/RegisterPendingResponse.cs` | `{ message: "Check your email." }` |

### Modified files

| File | Change |
|---|---|
| `WestcoastCars.Application/Services/IEmailService.cs` | Add `SendEmailVerificationAsync(string toEmail, string name, string confirmationLink)` |
| `WestcoastCars.Infrastructure/Services/EmailService.cs` | Implement `SendEmailVerificationAsync` |
| `WestcoastCars.Application/Services/IAuthService.cs` | Change `RegisterAsync` return type to `string` (pending message), add `ConfirmEmailAsync(string userId, string token)` returning `AuthenticationResult` |
| `WestcoastCars.Infrastructure/Services/AuthService.cs` | Register: generate token, send email, no JWT. Login: handle `IsNotAllowed → 403`. Add `ConfirmEmailAsync`. |
| `WestcoastCars.Infrastructure/DependencyInjection.cs` | Set `options.SignIn.RequireConfirmedEmail = true` |
| `WestcoastCars.Api/Controllers/AuthenticationController.cs` | Register → 202. Add `GET confirm-email` endpoint. Handle 403 case in login. |
| `WestcoastCars.Api/appsettings.json` | Add `"AppBaseUrl": ""` under a new `App` section |
| `WestcoastCars.Api/appsettings.Development.json` | Add `"AppBaseUrl": "http://localhost:5000"` |

---

## Step-by-step

### 1 — Config
Add to `appsettings.json`:
```json
"App": {
  "BaseUrl": ""
}
```
Add `AppOptions.cs` in `WestcoastCars.Api/Configurations/`.  
Register with `services.Configure<AppOptions>(config.GetSection("App"))`.

### 2 — `IEmailService` + `EmailService`
Add method:
```csharp
Task SendEmailVerificationAsync(string toEmail, string name, string confirmationLink);
```
Email body: plain text with the confirmation link. Swedish locale to match existing emails.

### 3 — `IAuthService` changes
```csharp
// RegisterAsync now returns void or a plain ack — no AuthenticationResult
Task RegisterAsync(string firstName, string lastName, string email, string password, string confirmationLinkBase);

// New
Task<AuthenticationResult> ConfirmEmailAsync(string userId, string token);
```

### 4 — `AuthService` changes

**RegisterAsync:**
```
1. Check email not already taken
2. CreateAsync user
3. AddClaimAsync firstName/lastName
4. AddToRoleAsync Customer
5. GenerateEmailConfirmationTokenAsync
6. Build link: {baseUrl}/api/auth/confirm-email?userId={id}&token={Uri.EscapeDataString(token)}
7. SendEmailVerificationAsync
8. Return (no JWT)
```

**LoginAsync:**
```
if result.IsNotAllowed → return a distinct result or throw EmailNotConfirmedException
```

**ConfirmEmailAsync:**
```
1. FindByIdAsync userId
2. ConfirmEmailAsync(user, token)
3. Get roles + claims
4. GenerateTokenAsync → return AuthenticationResult
```

### 5 — `DependencyInjection`
```csharp
options.SignIn.RequireConfirmedEmail = true;
```

### 6 — `AuthenticationController`

**Register** → pass `baseUrl` from `IOptions<AppOptions>`, return `202 Accepted`.

**New endpoint:**
```csharp
[HttpGet("confirm-email")]
public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string token)
```
Returns `200 + AuthenticationResponse` on success, `400` on invalid/expired token.

**Login** → detect unconfirmed case, return `403`.

### 7 — Tests
- `RegisterAsync` sends email and returns without JWT
- `ConfirmEmailAsync` happy path returns JWT
- `ConfirmEmailAsync` with bad token returns error
- `LoginAsync` before confirmation returns `IsNotAllowed`
- `LoginAsync` after confirmation succeeds

---

## Out of scope

- Resend verification email endpoint (can be added later)
- Token expiry customisation (Identity default is 24 h — acceptable)
- HTML email templates (plain text matches current style)
