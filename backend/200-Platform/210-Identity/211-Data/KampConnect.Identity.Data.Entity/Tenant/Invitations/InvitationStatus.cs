namespace KampConnect.Identity.Data.Entity.Tenant.Invitations;

/// <summary>
/// Status van een tenant-uitnodiging. Wordt als tekst bewaard (nvarchar(20)).
/// </summary>
public enum InvitationStatus
{
    /// <summary>Verstuurd, wacht op antwoord.</summary>
    Pending,
    /// <summary>Aanvaard: de gebruiker kreeg de rol binnen de tenant.</summary>
    Accepted,
    /// <summary>Ingetrokken door een tenant-admin.</summary>
    Cancelled,
    /// <summary>Geweigerd door de uitgenodigde persoon.</summary>
    Declined,
    /// <summary>Vervallen zonder antwoord.</summary>
    Expired
}
