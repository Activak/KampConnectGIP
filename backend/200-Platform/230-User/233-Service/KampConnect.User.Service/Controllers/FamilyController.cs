using KampConnect.Shared.Contracts.Authorization;
using KampConnect.User.Business.Contracts.ClassTypes.Results;
using KampConnect.User.Business.Contracts.Interfaces;
using KampConnect.User.Business.Contracts.Requests;
using KampConnect.User.Data.Entity.Family;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace KampConnect.User.Service.Controllers;

[ApiController]
[Route("api/family")]
[Authorize(Policy = AuthorizationConstants.RequiresAuthenticationPolicy)]
public class FamilyController(IFamilyService familyService, IFamilyInviteService inviteService) : ControllerBase
{
    public const string JoinRateLimitPolicy = "family-join";

    #region Families
    [HttpGet]
    public Task<IActionResult> GetFamilies(CancellationToken ct = default) =>
        Execute(authUserId => familyService.GetFamiliesAsync(authUserId, ct), r => Ok(r.Families));

    [HttpGet("{familyId:guid}")]
    public Task<IActionResult> GetFamily(Guid familyId, CancellationToken ct = default) =>
        Execute(authUserId => familyService.GetFamilyByIdAsync(familyId, authUserId, ct), r => Ok(r.Family));

    [HttpPost]
    public Task<IActionResult> CreateFamily(FamilyCreate request, CancellationToken ct = default) =>
        Execute(authUserId => familyService.AddFamilyAsync(authUserId, request, ct),
            r => CreatedAtAction(nameof(GetFamily), new { familyId = r.Family!.FamilyId }, r.Family));

    [HttpPatch("{familyId:guid}")]
    public Task<IActionResult> UpdateFamily(Guid familyId, FamilyUpdate request, CancellationToken ct = default) =>
        Execute(authUserId => familyService.UpdateFamilyAsync(familyId, authUserId, request, ct), r => Ok(r.Family));
    #endregion

    #region Members
    [HttpGet("{familyId:guid}/members")]
    public Task<IActionResult> GetMembers(Guid familyId, [FromQuery] FamilyMemberStatus? status, CancellationToken ct = default) =>
        Execute(authUserId => familyService.GetFamilyMembersAsync(familyId, authUserId, status, ct), r => Ok(r.Members));

    [HttpPost("{familyId:guid}/children")]
    public Task<IActionResult> AddChild(Guid familyId, ChildCreate request, CancellationToken ct = default) =>
        Execute(authUserId => familyService.AddChildAsync(authUserId, familyId, request, ct), r => Ok(r.Member));

    [HttpPatch("{familyId:guid}/members/{memberId:int}")]
    public Task<IActionResult> UpdateMember(Guid familyId, int memberId, MemberUpdate request, CancellationToken ct = default) =>
        Execute(authUserId => familyService.UpdateMemberAsync(familyId, authUserId, memberId, request, ct), r => Ok(r.Member));

    [HttpDelete("{familyId:guid}/members/{memberId:int}")]
    public Task<IActionResult> RemoveMember(Guid familyId, int memberId, CancellationToken ct = default) =>
        Execute(authUserId => familyService.RemoveMemberAsync(familyId, authUserId, memberId, ct), _ => NoContent());

    [HttpPost("{familyId:guid}/members/{memberId:int}/approve")]
    public Task<IActionResult> ApproveMember(Guid familyId, int memberId, CancellationToken ct = default) =>
        Execute(authUserId => familyService.ApproveMemberAsync(familyId, authUserId, memberId, ct), r => Ok(r.Member));

    [HttpPost("{familyId:guid}/members/{memberId:int}/reject")]
    public Task<IActionResult> RejectMember(Guid familyId, int memberId, CancellationToken ct = default) =>
        Execute(authUserId => familyService.RejectMemberAsync(familyId, authUserId, memberId, ct), r => Ok(r.Member));

    // Rate limited tegen het raden van korte codes
    [HttpPost("join")]
    [EnableRateLimiting(JoinRateLimitPolicy)]
    public Task<IActionResult> Join(JoinFamily request, CancellationToken ct = default) =>
        Execute(authUserId => familyService.JoinFamilyAsync(request, authUserId, ct), r => Accepted(r.Member));
    #endregion

    #region Invites
    [HttpGet("{familyId:guid}/invites")]
    public Task<IActionResult> GetInvites(Guid familyId, CancellationToken ct = default) =>
        Execute(authUserId => inviteService.GetActiveInvitesAsync(familyId, authUserId, ct), r => Ok(r.Invites));

    // Code wordt enkel hier in plaintext teruggegeven
    [HttpPost("{familyId:guid}/invites")]
    public Task<IActionResult> CreateInvite(Guid familyId, CancellationToken ct = default) =>
        Execute(authUserId => inviteService.CreateInviteAsync(familyId, authUserId, ct), r => Ok(r.CreatedInvite));

    [HttpDelete("{familyId:guid}/invites/{inviteId:int}")]
    public Task<IActionResult> RevokeInvite(Guid familyId, int inviteId, CancellationToken ct = default) =>
        Execute(authUserId => inviteService.RevokeInviteAsync(familyId, authUserId, inviteId, ct), _ => NoContent());
    #endregion

    #region Helpers
    // Haalt authUserId op, roept de business aan en vertaalt fouten naar HTTP
    private async Task<IActionResult> Execute(Func<Guid, Task<FamilyResult>> action, Func<FamilyResult, IActionResult> onSuccess)
    {
        if (!Guid.TryParse(User.FindFirst(KampConnectClaimTypes.Subject)?.Value, out var authUserId))
        {
            return Unauthorized();
        }

        var result = await action(authUserId);
        return result.Succeeded ? onSuccess(result) : ToError(result);
    }

    private IActionResult ToError(FamilyResult result) => result.Error switch
    {
        FamilyError.ValidationFailed => ValidationProblem(new ValidationProblemDetails(result.ValidationErrors ?? new Dictionary<string, string[]>())),
        FamilyError.UserNotFound or FamilyError.FamilyNotFound or FamilyError.MemberNotFound or FamilyError.InviteNotFound => NotFound(new { message = result.Error }),
        FamilyError.NotAuthorized => Forbid(),
        FamilyError.InvalidOrExpiredCode or FamilyError.InvitesDisabled => BadRequest(new { message = result.Error }),
        _ => Conflict(new { message = result.Error })
    };
    #endregion
}
