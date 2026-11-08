using KampConnect.User.Data.Entity.Family;

namespace KampConnect.User.Data.Interface.Repository;

public interface IFamilyInviteRepository
{
    // Opzoeken bij het toetreden, inclusief familie (voor invite_enabled)
    public Task<UserFamilyInvite?> GetByCodeHashAsync(byte[] codeHash, CancellationToken ct = default);

    public Task<UserFamilyInvite?> GetByIdAsync(Guid familyId, int familyInviteId, CancellationToken ct = default);

    // Niet verlopen en niet ingetrokken op tijdstip 'now'
    public Task<List<UserFamilyInvite>> GetActiveByFamilyIdAsync(Guid familyId, DateTime now, CancellationToken ct = default);

    public void Add(UserFamilyInvite invite);

    public Task SaveChangesAsync(CancellationToken ct = default);
}
