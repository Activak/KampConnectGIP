namespace KampConnect.Identity.Business.Contracts.ClassTypes;

/// <summary>Fouten die de uitnodigings-endpoints kunnen teruggeven.</summary>
public enum InvitationError
{
    None,
    /// <summary>Invoer ongeldig (zie <see cref="InvitationResult.Errors"/>) → 400.</summary>
    Validation,
    /// <summary>Uitnodiging niet gevonden (of hoort niet bij deze tenant) → 404.</summary>
    NotFound,
    /// <summary>Token ongeldig, verlopen of al gebruikt → 400.</summary>
    InvalidToken,
    /// <summary>De uitnodiging is voor een ander e-mailadres dan dat van de ingelogde gebruiker → 403.</summary>
    EmailMismatch,
    /// <summary>Conflict met de huidige toestand (bv. al openstaand, al lid, niet meer Pending) → 409.</summary>
    Conflict
}
