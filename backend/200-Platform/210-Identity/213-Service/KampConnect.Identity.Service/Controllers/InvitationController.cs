using KampConnect.Identity.Business.Contracts.ClassTypes;
using KampConnect.Identity.Business.Contracts.Interfaces;
using KampConnect.Identity.Business.Contracts.Requests;
using KampConnect.Shared.Contracts.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using OpenIddict.Validation.AspNetCore;

namespace KampConnect.Identity.Service.Controllers;

/// <summary>
/// /api/invitations — uitnodigingen om lid te worden van een tenant.
/// <list type="bullet">
/// <item>Beheer (aanmaken, lijst, intrekken, opnieuw versturen): enkel tenant-admins, voor de tenant uit hun token.</item>
/// <item>Bekijken en weigeren via het token uit de mail: anoniem.</item>
/// <item>Aanvaarden: ingelogde gebruiker. Nieuwe gebruikers registreren met het token via /api/account/register.</item>
/// </list>
/// </summary>
[ApiController]
[Route("api/invitations")]
public class InvitationController(IInvitationService invitationService) : ControllerBase
{
    #region Beheer (tenant-admin)

    /// <summary>Uitnodiging aanmaken → 201 · 400 validatie · 404 tenant · 409 al openstaand/al lid.</summary>
    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme,
               Policy = AuthorizationConstants.RequiresTenantAdminPolicy)]
    [HttpPost]
    public async Task<IActionResult> CreateInvitation(CreateInvitationRequest request, CancellationToken ct)
    {
        if (!TryGetUserAndTenant(out var userId, out var tenantId, out var error)) return error!;

        var result = await invitationService.CreateInvitationAsync(userId, tenantId, request, ct);
        return result.Succeeded
            ? StatusCode(StatusCodes.Status201Created, result.Invitations![0])
            : ToErrorResult(result);
    }

    /// <summary>Alle uitnodigingen van de tenant → 200.</summary>
    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme,
               Policy = AuthorizationConstants.RequiresTenantAdminPolicy)]
    [HttpGet]
    public async Task<IActionResult> GetInvitations(CancellationToken ct)
    {
        if (!TryGetUserAndTenant(out _, out var tenantId, out var error)) return error!;

        var result = await invitationService.GetInvitationsAsync(tenantId, ct);
        return result.Succeeded ? Ok(result.Invitations) : ToErrorResult(result);
    }

    /// <summary>Uitnodiging intrekken → 200 · 404 · 409 niet meer openstaand.</summary>
    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme,
               Policy = AuthorizationConstants.RequiresTenantAdminPolicy)]
    [HttpPost("{invitationId:int}/cancel")]
    public async Task<IActionResult> CancelInvitation(int invitationId, CancellationToken ct)
    {
        if (!TryGetUserAndTenant(out var userId, out var tenantId, out var error)) return error!;

        var result = await invitationService.CancelInvitationAsync(userId, tenantId, invitationId, ct);
        return result.Succeeded ? Ok(result.Invitations![0]) : ToErrorResult(result);
    }

    /// <summary>Uitnodiging opnieuw versturen (nieuw token + vervaldatum) → 200 · 404 · 409.</summary>
    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme,
               Policy = AuthorizationConstants.RequiresTenantAdminPolicy)]
    [HttpPost("{invitationId:int}/resend")]
    public async Task<IActionResult> ResendInvitation(int invitationId, CancellationToken ct)
    {
        if (!TryGetUserAndTenant(out var userId, out var tenantId, out var error)) return error!;

        var result = await invitationService.ResendInvitationAsync(userId, tenantId, invitationId, ct);
        return result.Succeeded ? Ok(result.Invitations![0]) : ToErrorResult(result);
    }

    #endregion

    #region Uitgenodigde persoon

    /// <summary>Uitnodiging bekijken via het token (uitnodigingspagina) → 200 · 400 ongeldig/verlopen.</summary>
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicies.Auth)]
    [HttpGet("by-token")]
    public async Task<IActionResult> GetInvitationByToken([FromQuery] string token, CancellationToken ct)
    {
        var result = await invitationService.GetInvitationByTokenAsync(token, ct);
        return result.Succeeded ? Ok(result.Invitations![0]) : ToErrorResult(result);
    }

    /// <summary>Uitnodiging aanvaarden (bestaande, ingelogde gebruiker) → 200 · 400 token · 403 ander e-mailadres.</summary>
    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    [HttpPost("accept")]
    public async Task<IActionResult> AcceptInvitation(AcceptInvitationRequest request, CancellationToken ct)
    {
        if (!Guid.TryParse(User.FindFirst(KampConnectClaimTypes.Subject)?.Value, out var userId))
        {
            return Unauthorized();
        }

        var result = await invitationService.AcceptInvitationAsync(userId, request, ct);
        return result.Succeeded ? Ok(result.Invitations![0]) : ToErrorResult(result);
    }

    /// <summary>Uitnodiging weigeren via het token → 204 · 400 ongeldig/verlopen.</summary>
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicies.Auth)]
    [HttpPost("decline")]
    public async Task<IActionResult> DeclineInvitation(DeclineInvitationRequest request, CancellationToken ct)
    {
        var result = await invitationService.DeclineInvitationAsync(request, ct);
        return result.Succeeded ? NoContent() : ToErrorResult(result);
    }

    #endregion

    #region Helpers

    private bool TryGetUserAndTenant(out Guid userId, out int tenantId, out IActionResult? error)
    {
        tenantId = 0;
        error = null;

        if (!Guid.TryParse(User.FindFirst(KampConnectClaimTypes.Subject)?.Value, out userId))
        {
            error = Unauthorized();
            return false;
        }

        if (!int.TryParse(User.FindFirst(KampConnectClaimTypes.TenantId)?.Value, out tenantId))
        {
            error = Forbid(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
            return false;
        }

        return true;
    }

    /// <summary>Zet een mislukt <see cref="InvitationResult"/> om naar ProblemDetails met de juiste statuscode.</summary>
    private IActionResult ToErrorResult(InvitationResult result)
    {
        var detail = result.Errors is not null && result.Errors.TryGetValue("Error", out var messages)
            ? string.Join(" ", messages)
            : null;

        return result.Error switch
        {
            InvitationError.Validation => ValidationProblem(new ValidationProblemDetails(result.Errors ?? new Dictionary<string, string[]>())
            {
                Title = "Validatiefout",
                Status = StatusCodes.Status400BadRequest
            }),
            InvitationError.NotFound => Problem(title: "Niet gevonden", detail: detail, statusCode: StatusCodes.Status404NotFound),
            InvitationError.InvalidToken => Problem(title: "Uitnodiging ongeldig of verlopen", detail: detail, statusCode: StatusCodes.Status400BadRequest),
            InvitationError.EmailMismatch => Problem(title: "Uitnodiging hoort bij een ander e-mailadres", detail: detail, statusCode: StatusCodes.Status403Forbidden),
            InvitationError.Conflict => Problem(title: "Conflict", detail: detail, statusCode: StatusCodes.Status409Conflict),
            _ => Problem(statusCode: StatusCodes.Status500InternalServerError)
        };
    }

    #endregion
}
