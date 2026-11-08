using KampConnect.User.Business.Contracts.ClassTypes.Results;
using KampConnect.User.Business.Contracts.Requests;
using KampConnect.User.Business.Logic.Helpers;
using KampConnect.User.Business.Logic.Implementation;
using KampConnect.User.Data.Entity.Family;
using KampConnect.User.Tests.Fakes;
using Microsoft.Extensions.Logging.Abstractions;

namespace KampConnect.User.Tests.Family;

public class FamilyInviteServiceTests
{
    private readonly FakeStore _store = new();
    private readonly FakeTimeProvider _time = new(new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero));
    private readonly FamilyInviteService _sut;
    private readonly FamilyService _familyService;

    public FamilyInviteServiceTests()
    {
        var familyRepo = new FakeFamilyRepository(_store);
        var inviteRepo = new FakeFamilyInviteRepository(_store);
        var userRepo = new FakeUserRepository(_store);
        _sut = new FamilyInviteService(familyRepo, inviteRepo, userRepo, _time, NullLogger<FamilyInviteService>.Instance);
        _familyService = new FamilyService(familyRepo, inviteRepo, userRepo, _time, NullLogger<FamilyService>.Instance);
    }

    [Fact]
    public async Task CreateInvite_ReturnsCodeAndStoresOnlyHash()
    {
        var guardian = _store.AddUser();
        var family = _store.AddFamily();
        _store.AddMember(family, guardian, FamilyRoleType.guardian, FamilyMemberStatus.active);

        var result = await _sut.CreateInviteAsync(family.family_id, guardian.auth_user_id!.Value);

        Assert.True(result.Succeeded);
        var code = result.CreatedInvite!.Code;
        Assert.Equal(InviteCodeHelper.CodeLength, code.Length);
        Assert.All(code, c => Assert.Contains(c, InviteCodeHelper.Alphabet));
        var invite = Assert.Single(_store.Invites);
        Assert.Equal(InviteCodeHelper.Hash(code), invite.code_hash);
        Assert.Equal(_time.Now.UtcDateTime.Add(InviteCodeHelper.Lifetime), invite.expires_at);
    }

    [Fact]
    public async Task CreateInvite_NonGuardian_ReturnsNotAuthorized()
    {
        var child = _store.AddUser();
        var family = _store.AddFamily();
        _store.AddMember(family, child, FamilyRoleType.child, FamilyMemberStatus.active);

        var result = await _sut.CreateInviteAsync(family.family_id, child.auth_user_id!.Value);

        Assert.Equal(FamilyError.NotAuthorized, result.Error);
        Assert.Empty(_store.Invites);
    }

    [Fact]
    public async Task CreateInvite_InvitesDisabled_ReturnsInvitesDisabled()
    {
        var guardian = _store.AddUser();
        var family = _store.AddFamily(inviteEnabled: false);
        _store.AddMember(family, guardian, FamilyRoleType.guardian, FamilyMemberStatus.active);

        var result = await _sut.CreateInviteAsync(family.family_id, guardian.auth_user_id!.Value);

        Assert.Equal(FamilyError.InvitesDisabled, result.Error);
    }

    [Fact]
    public async Task GetActiveInvites_ExcludesExpiredAndRevoked()
    {
        var guardian = _store.AddUser();
        var family = _store.AddFamily();
        _store.AddMember(family, guardian, FamilyRoleType.guardian, FamilyMemberStatus.active);
        await _sut.CreateInviteAsync(family.family_id, guardian.auth_user_id!.Value);
        var revoked = await _sut.CreateInviteAsync(family.family_id, guardian.auth_user_id!.Value);
        await _sut.RevokeInviteAsync(family.family_id, guardian.auth_user_id!.Value, revoked.CreatedInvite!.FamilyInviteId);

        var active = await _sut.GetActiveInvitesAsync(family.family_id, guardian.auth_user_id!.Value);
        Assert.Single(active.Invites!);

        _time.Now = _time.Now.Add(InviteCodeHelper.Lifetime).AddSeconds(1);
        var afterExpiry = await _sut.GetActiveInvitesAsync(family.family_id, guardian.auth_user_id!.Value);
        Assert.Empty(afterExpiry.Invites!);
    }

    [Fact]
    public async Task RevokeInvite_Twice_ReturnsInviteNotFound()
    {
        var guardian = _store.AddUser();
        var family = _store.AddFamily();
        _store.AddMember(family, guardian, FamilyRoleType.guardian, FamilyMemberStatus.active);
        var created = await _sut.CreateInviteAsync(family.family_id, guardian.auth_user_id!.Value);
        var id = created.CreatedInvite!.FamilyInviteId;

        await _sut.RevokeInviteAsync(family.family_id, guardian.auth_user_id!.Value, id);
        var second = await _sut.RevokeInviteAsync(family.family_id, guardian.auth_user_id!.Value, id);

        Assert.Equal(FamilyError.InviteNotFound, second.Error);
    }

    [Fact]
    public async Task FullFlow_CreateJoinApprove_MakesMemberActive()
    {
        var guardian = _store.AddUser();
        var family = _store.AddFamily();
        _store.AddMember(family, guardian, FamilyRoleType.guardian, FamilyMemberStatus.active);
        var joiner = _store.AddUser(firstName: "Joiner");

        var created = await _sut.CreateInviteAsync(family.family_id, guardian.auth_user_id!.Value);
        var joined = await _familyService.JoinFamilyAsync(new JoinFamily(created.CreatedInvite!.Code.ToLowerInvariant()), joiner.auth_user_id!.Value);
        var approved = await _familyService.ApproveMemberAsync(family.family_id, guardian.auth_user_id!.Value, joined.Member!.FamilyMemberId);

        Assert.True(approved.Succeeded);
        Assert.Equal(FamilyMemberStatus.active, approved.Member!.Status);
        Assert.Equal("Joiner", approved.Member.FirstName);
    }

    [Fact]
    public async Task Join_RevokedInvite_Fails()
    {
        var guardian = _store.AddUser();
        var family = _store.AddFamily();
        _store.AddMember(family, guardian, FamilyRoleType.guardian, FamilyMemberStatus.active);
        var joiner = _store.AddUser();

        var created = await _sut.CreateInviteAsync(family.family_id, guardian.auth_user_id!.Value);
        await _sut.RevokeInviteAsync(family.family_id, guardian.auth_user_id!.Value, created.CreatedInvite!.FamilyInviteId);
        var result = await _familyService.JoinFamilyAsync(new JoinFamily(created.CreatedInvite.Code), joiner.auth_user_id!.Value);

        Assert.Equal(FamilyError.InvalidOrExpiredCode, result.Error);
    }
}
