using KampConnect.Identity.Data.Entity.Tenant.Invitations;

namespace KampConnect.Identity.Business.Contracts.Dto;

/// <summary>
/// Uitnodiging zoals de API ze teruggeeft. Het token zelf zit er nooit in (enkel in de mail).
/// </summary>
public class InvitationDto
{
    public int TenantInvitationId { get; set; }
    public int TenantId { get; set; }
    public string? TenantName { get; set; }
    public string Email { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public string? RoleCode { get; set; }
    public string? RoleName { get; set; }
    public InvitationStatus Status { get; set; }
    public Guid SentByUserId { get; set; }
    public Guid? AcceptedByUserId { get; set; }
    public Guid? CancelledByUserId { get; set; }
    public DateTime ExpiryTimestamp { get; set; }
    public DateTime CreatedTimestamp { get; set; }
    public DateTime? AcceptedTimestamp { get; set; }
    public DateTime? CancelledTimestamp { get; set; }

    /// <summary>Nog Pending, maar de vervaldatum is voorbij.</summary>
    public bool IsExpired => Status == InvitationStatus.Pending && ExpiryTimestamp <= DateTime.UtcNow;
}
