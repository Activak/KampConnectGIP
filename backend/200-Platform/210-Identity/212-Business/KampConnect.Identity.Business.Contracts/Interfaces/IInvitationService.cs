using KampConnect.Identity.Business.Contracts.ClassTypes;
using KampConnect.Identity.Business.Contracts.Requests;

namespace KampConnect.Identity.Business.Contracts.Interfaces;

/// <summary>
/// Uitnodigingen om lid te worden van een tenant (met een rol).
/// Het token staat enkel in de mail; in de database bewaren we de hash.
/// </summary>
public interface IInvitationService
{
    /// <summary>Maakt een uitnodiging aan en verstuurt de mail.</summary>
    Task<InvitationResult> CreateInvitationAsync(Guid sentByUserId, int tenantId, CreateInvitationRequest request, CancellationToken ct = default);

    /// <summary>Alle uitnodigingen van een tenant.</summary>
    Task<InvitationResult> GetInvitationsAsync(int tenantId, CancellationToken ct = default);

    /// <summary>Uitnodiging opzoeken via het token (voor de uitnodigingspagina in de frontend).</summary>
    Task<InvitationResult> GetInvitationByTokenAsync(string token, CancellationToken ct = default);

    /// <summary>Trekt een openstaande uitnodiging in.</summary>
    Task<InvitationResult> CancelInvitationAsync(Guid cancelledByUserId, int tenantId, int invitationId, CancellationToken ct = default);

    /// <summary>Maakt een nieuw token (en nieuwe vervaldatum) en verstuurt de mail opnieuw. Het oude token werkt niet meer.</summary>
    Task<InvitationResult> ResendInvitationAsync(Guid userId, int tenantId, int invitationId, CancellationToken ct = default);

    /// <summary>Een ingelogde gebruiker aanvaardt de uitnodiging → krijgt de rol binnen de tenant.</summary>
    Task<InvitationResult> AcceptInvitationAsync(Guid userId, AcceptInvitationRequest request, CancellationToken ct = default);

    /// <summary>De uitgenodigde persoon weigert de uitnodiging.</summary>
    Task<InvitationResult> DeclineInvitationAsync(DeclineInvitationRequest request, CancellationToken ct = default);
}
