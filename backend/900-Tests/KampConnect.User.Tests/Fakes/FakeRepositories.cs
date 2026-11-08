using KampConnect.User.Data.Entity.Family;
using KampConnect.User.Data.Interface.Repository;

namespace KampConnect.User.Tests.Fakes;

using Family = KampConnect.User.Data.Entity.Family.Family;
using User = KampConnect.User.Data.Entity.User.User;

// Eenvoudige in-memory opslag gedeeld door de fake repositories
public class FakeStore
{
    public List<User> Users { get; } = [];
    public List<Family> Families { get; } = [];
    public List<UserFamilyMember> Members { get; } = [];
    public List<UserFamilyInvite> Invites { get; } = [];
    public int SaveCount { get; set; }

    private int _memberId;
    private int _inviteId;

    public void AssignIds()
    {
        foreach (var m in Members.Where(m => m.family_member_id == 0))
        {
            m.family_member_id = ++_memberId;
        }

        foreach (var i in Invites.Where(i => i.family_invite_id == 0))
        {
            i.family_invite_id = ++_inviteId;
        }
    }

    public User AddUser(Guid? authUserId = null, string firstName = "Test")
    {
        var user = new User
        {
            user_id = Guid.NewGuid(),
            auth_user_id = authUserId ?? Guid.NewGuid(),
            first_name = firstName,
            last_name = "User",
            birth_date = new DateOnly(1990, 1, 1),
            preferred_language = "nl"
        };
        Users.Add(user);
        return user;
    }

    public Family AddFamily(bool inviteEnabled = true)
    {
        var family = new Family { family_id = Guid.NewGuid(), name = "Familie Test", invite_enabled = inviteEnabled };
        Families.Add(family);
        return family;
    }

    public UserFamilyMember AddMember(Family family, User user, FamilyRoleType role, FamilyMemberStatus status)
    {
        var member = new UserFamilyMember
        {
            family_id = family.family_id,
            user_id = user.user_id,
            role_type = role,
            status = status,
            is_primary_contact = false,
            Family = family,
            User = user
        };
        Members.Add(member);
        AssignIds();
        return member;
    }
}

public class FakeUserRepository(FakeStore store) : IUserRepository
{
    public Task<User?> GetUserByAuthIdAsync(Guid authUserId, CancellationToken ct = default) =>
        Task.FromResult(store.Users.FirstOrDefault(u => u.auth_user_id == authUserId));

    public void Add(User user) => store.Users.Add(user);

    public Task SaveChangesAsync(CancellationToken ct = default)
    {
        store.SaveCount++;
        store.AssignIds();
        return Task.CompletedTask;
    }
}

public class FakeFamilyRepository(FakeStore store) : IFamilyRepository
{
    public Task<List<UserFamilyMember>> GetActiveMembershipsByUserIdAsync(Guid userId, CancellationToken ct = default) =>
        Task.FromResult(store.Members.Where(m => m.user_id == userId && m.status == FamilyMemberStatus.active).ToList());

    public Task<Family?> GetFamilyByIdAsync(Guid familyId, CancellationToken ct = default) =>
        Task.FromResult(store.Families.FirstOrDefault(f => f.family_id == familyId));

    public Task<List<UserFamilyMember>> GetMembersByFamilyIdAsync(Guid familyId, FamilyMemberStatus? status = null, CancellationToken ct = default) =>
        Task.FromResult(store.Members.Where(m => m.family_id == familyId && (status == null || m.status == status)).ToList());

    public Task<UserFamilyMember?> GetMembershipAsync(Guid familyId, Guid userId, CancellationToken ct = default) =>
        Task.FromResult(store.Members.FirstOrDefault(m => m.family_id == familyId && m.user_id == userId));

    public Task<UserFamilyMember?> GetMemberByIdAsync(Guid familyId, int familyMemberId, CancellationToken ct = default) =>
        Task.FromResult(store.Members.FirstOrDefault(m => m.family_id == familyId && m.family_member_id == familyMemberId));

    public Task<int> CountActiveGuardiansAsync(Guid familyId, CancellationToken ct = default) =>
        Task.FromResult(store.Members.Count(m => m.family_id == familyId && m.role_type == FamilyRoleType.guardian && m.status == FamilyMemberStatus.active));

    public void AddFamily(Family family) => store.Families.Add(family);

    public void AddMember(UserFamilyMember member) => store.Members.Add(member);

    public Task SaveChangesAsync(CancellationToken ct = default)
    {
        store.SaveCount++;
        store.AssignIds();
        return Task.CompletedTask;
    }
}

public class FakeFamilyInviteRepository(FakeStore store) : IFamilyInviteRepository
{
    public Task<UserFamilyInvite?> GetByCodeHashAsync(byte[] codeHash, CancellationToken ct = default)
    {
        var invite = store.Invites.FirstOrDefault(i => i.code_hash.SequenceEqual(codeHash));
        if (invite is not null)
        {
            invite.Family = store.Families.First(f => f.family_id == invite.family_id);
        }
        return Task.FromResult(invite);
    }

    public Task<UserFamilyInvite?> GetByIdAsync(Guid familyId, int familyInviteId, CancellationToken ct = default) =>
        Task.FromResult(store.Invites.FirstOrDefault(i => i.family_id == familyId && i.family_invite_id == familyInviteId));

    public Task<List<UserFamilyInvite>> GetActiveByFamilyIdAsync(Guid familyId, DateTime now, CancellationToken ct = default) =>
        Task.FromResult(store.Invites.Where(i => i.family_id == familyId && i.revoked_at == null && i.expires_at > now).ToList());

    public void Add(UserFamilyInvite invite) => store.Invites.Add(invite);

    public Task SaveChangesAsync(CancellationToken ct = default)
    {
        store.SaveCount++;
        store.AssignIds();
        return Task.CompletedTask;
    }
}

// Vaste klok zodat expiratie testbaar is
public class FakeTimeProvider(DateTimeOffset now) : TimeProvider
{
    public DateTimeOffset Now { get; set; } = now;

    public override DateTimeOffset GetUtcNow() => Now;
}
