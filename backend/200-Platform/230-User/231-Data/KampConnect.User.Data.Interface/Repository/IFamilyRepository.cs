using KampConnect.User.Data.Entity.Family;

namespace KampConnect.User.Data.Interface.Repository;

public interface IFamilyRepository
{
    // Actieve lidmaatschappen van een gebruiker, inclusief familie
    public Task<List<UserFamilyMember>> GetActiveMembershipsByUserIdAsync(Guid userId, CancellationToken ct = default);

    public Task<Family?> GetFamilyByIdAsync(Guid familyId, CancellationToken ct = default);

    // Leden inclusief gebruikersgegevens, optioneel gefilterd op status
    public Task<List<UserFamilyMember>> GetMembersByFamilyIdAsync(Guid familyId, FamilyMemberStatus? status = null, CancellationToken ct = default);

    // Lidmaatschap van een gebruiker in een familie, ongeacht status (autorisatie + dubbele join)
    public Task<UserFamilyMember?> GetMembershipAsync(Guid familyId, Guid userId, CancellationToken ct = default);

    public Task<UserFamilyMember?> GetMemberByIdAsync(Guid familyId, int familyMemberId, CancellationToken ct = default);

    public Task<int> CountActiveGuardiansAsync(Guid familyId, CancellationToken ct = default);

    public void AddFamily(Family family);

    public void AddMember(UserFamilyMember member);

    public Task SaveChangesAsync(CancellationToken ct = default);
}
