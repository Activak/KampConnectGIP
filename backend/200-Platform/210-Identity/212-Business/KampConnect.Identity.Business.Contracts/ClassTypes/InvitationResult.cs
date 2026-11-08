using KampConnect.Identity.Business.Contracts.Dto;

namespace KampConnect.Identity.Business.Contracts.ClassTypes;

/// <summary>
/// Resultaat van een uitnodigingsactie.
/// <see cref="Errors"/> heeft dezelfde vorm als "errors" in ProblemDetails (veldnaam → fouten).
/// </summary>
public record InvitationResult(bool Succeeded,
    List<InvitationDto>? Invitations = null,
    IDictionary<string, string[]>? Errors = null,
    InvitationError Error = InvitationError.None)
{
    public static InvitationResult Success(List<InvitationDto> invitations) => new InvitationResult(true, invitations);
    public static InvitationResult Success(InvitationDto invitation) => new InvitationResult(true, [invitation]);
    public static InvitationResult Fail(IDictionary<string, string[]> errors) => new InvitationResult(false, null, errors, InvitationError.Validation);
    public static InvitationResult Fail(InvitationError error, string errorMessage) =>
        new InvitationResult(false, null, new Dictionary<string, string[]> { { "Error", new[] { errorMessage } } }, error);
}
