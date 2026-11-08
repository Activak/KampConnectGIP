using KampConnect.User.Business.Contracts.ClassTypes.Results;
using KampConnect.User.Business.Contracts.Requests;
using KampConnect.User.Business.Logic.Helpers;
using KampConnect.User.Business.Logic.Implementation;
using KampConnect.User.Data.Entity.Family;
using KampConnect.User.Tests.Fakes;
using Microsoft.Extensions.Logging.Abstractions;

namespace KampConnect.User.Tests.Family;

public class FamilyServiceTests
{
    private readonly FakeStore _store = new();
    private readonly FakeTimeProvider _time = new(new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero));
    private readonly FamilyService _sut;

    public FamilyServiceTests()
    {
        _sut = new FamilyService(
            new FakeFamilyRepository(_store),
            new FakeFamilyInviteRepository(_store),
            new FakeUserRepository(_store),
            _time,
            NullLogger<FamilyService>.Instance);
    }

    private UserFamilyInvite AddInvite(Data.Entity.Family.Family family, string code, TimeSpan? validFor = null)
    {
        var invite = new UserFamilyInvite
        {
            family_id = family.family_id,
            code_hash = InviteCodeHelper.Hash(code),
            expires_at = _time.Now.UtcDateTime.Add(validFor ?? TimeSpan.FromMinutes(15)),
            used_count = 0
        };
        _store.Invites.Add(invite);
        _store.AssignIds();
        return invite;
    }

    #region AddFamily
    [Fact]
    public async Task AddFamily_CreatesFamilyWithCreatorAsActiveGuardian()
    {
        var user = _store.AddUser();

        var result = await _sut.AddFamilyAsync(user.auth_user_id!.Value, new FamilyCreate("  Peeters  "));

        Assert.True(result.Succeeded);
        Assert.Equal("Peeters", result.Family!.Name);
        Assert.Equal(FamilyRoleType.guardian, result.Family.MyRole);
        var member = Assert.Single(_store.Members);
        Assert.Equal(FamilyMemberStatus.active, member.status);
        Assert.True(member.is_primary_contact);
        Assert.Equal(1, _store.SaveCount);
    }

    [Fact]
    public async Task AddFamily_UnknownUser_ReturnsUserNotFound()
    {
        var result = await _sut.AddFamilyAsync(Guid.NewGuid(), new FamilyCreate("Peeters"));

        Assert.Equal(FamilyError.UserNotFound, result.Error);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task AddFamily_EmptyName_ReturnsValidationFailed(string name)
    {
        var user = _store.AddUser();

        var result = await _sut.AddFamilyAsync(user.auth_user_id!.Value, new FamilyCreate(name));

        Assert.Equal(FamilyError.ValidationFailed, result.Error);
        Assert.Contains("Name", result.ValidationErrors!.Keys);
    }
    #endregion

    #region Get
    [Fact]
    public async Task GetFamilies_ReturnsOnlyActiveMemberships()
    {
        var user = _store.AddUser();
        var active = _store.AddFamily();
        var pending = _store.AddFamily();
        _store.AddMember(active, user, FamilyRoleType.guardian, FamilyMemberStatus.active);
        _store.AddMember(pending, user, FamilyRoleType.guardian, FamilyMemberStatus.pending);

        var result = await _sut.GetFamiliesAsync(user.auth_user_id!.Value);

        var family = Assert.Single(result.Families!);
        Assert.Equal(active.family_id, family.FamilyId);
    }

    [Fact]
    public async Task GetFamilyById_NonMember_ReturnsNotAuthorized()
    {
        var user = _store.AddUser();
        var family = _store.AddFamily();

        var result = await _sut.GetFamilyByIdAsync(family.family_id, user.auth_user_id!.Value);

        Assert.Equal(FamilyError.NotAuthorized, result.Error);
    }

    [Fact]
    public async Task GetFamilyById_UnknownFamily_ReturnsFamilyNotFound()
    {
        var user = _store.AddUser();

        var result = await _sut.GetFamilyByIdAsync(Guid.NewGuid(), user.auth_user_id!.Value);

        Assert.Equal(FamilyError.FamilyNotFound, result.Error);
    }

    [Fact]
    public async Task GetMembers_PendingMemberCannotSeeFamily()
    {
        var user = _store.AddUser();
        var family = _store.AddFamily();
        _store.AddMember(family, user, FamilyRoleType.guardian, FamilyMemberStatus.pending);

        var result = await _sut.GetFamilyMembersAsync(family.family_id, user.auth_user_id!.Value, null);

        Assert.Equal(FamilyError.NotAuthorized, result.Error);
    }

    [Fact]
    public async Task GetMembers_GuardianCanFilterPending()
    {
        var guardian = _store.AddUser();
        var family = _store.AddFamily();
        _store.AddMember(family, guardian, FamilyRoleType.guardian, FamilyMemberStatus.active);
        _store.AddMember(family, _store.AddUser(), FamilyRoleType.guardian, FamilyMemberStatus.pending);

        var result = await _sut.GetFamilyMembersAsync(family.family_id, guardian.auth_user_id!.Value, FamilyMemberStatus.pending);

        var member = Assert.Single(result.Members!);
        Assert.Equal(FamilyMemberStatus.pending, member.Status);
    }
    #endregion

    #region UpdateFamily
    [Fact]
    public async Task UpdateFamily_Guardian_UpdatesFields()
    {
        var guardian = _store.AddUser();
        var family = _store.AddFamily();
        _store.AddMember(family, guardian, FamilyRoleType.guardian, FamilyMemberStatus.active);

        var result = await _sut.UpdateFamilyAsync(family.family_id, guardian.auth_user_id!.Value, new FamilyUpdate("Nieuw", false));

        Assert.True(result.Succeeded);
        Assert.Equal("Nieuw", family.name);
        Assert.False(family.invite_enabled);
    }

    [Fact]
    public async Task UpdateFamily_Child_ReturnsNotAuthorized()
    {
        var child = _store.AddUser();
        var family = _store.AddFamily();
        _store.AddMember(family, child, FamilyRoleType.child, FamilyMemberStatus.active);

        var result = await _sut.UpdateFamilyAsync(family.family_id, child.auth_user_id!.Value, new FamilyUpdate("X", null));

        Assert.Equal(FamilyError.NotAuthorized, result.Error);
    }
    #endregion

    #region UpdateMember
    [Fact]
    public async Task UpdateMember_DemotingLastGuardian_ReturnsLastGuardian()
    {
        var guardian = _store.AddUser();
        var family = _store.AddFamily();
        var member = _store.AddMember(family, guardian, FamilyRoleType.guardian, FamilyMemberStatus.active);

        var result = await _sut.UpdateMemberAsync(family.family_id, guardian.auth_user_id!.Value, member.family_member_id, new MemberUpdate(FamilyRoleType.child, null));

        Assert.Equal(FamilyError.LastGuardian, result.Error);
        Assert.Equal(FamilyRoleType.guardian, member.role_type);
    }

    [Fact]
    public async Task UpdateMember_SetsPrimaryContact()
    {
        var guardian = _store.AddUser();
        var family = _store.AddFamily();
        _store.AddMember(family, guardian, FamilyRoleType.guardian, FamilyMemberStatus.active);
        var other = _store.AddMember(family, _store.AddUser(), FamilyRoleType.guardian, FamilyMemberStatus.active);

        var result = await _sut.UpdateMemberAsync(family.family_id, guardian.auth_user_id!.Value, other.family_member_id, new MemberUpdate(null, true));

        Assert.True(result.Succeeded);
        Assert.True(other.is_primary_contact);
    }
    #endregion

    #region AddChild
    [Fact]
    public async Task AddChild_CreatesUserWithoutLoginAndActiveMembership()
    {
        var guardian = _store.AddUser();
        var family = _store.AddFamily();
        _store.AddMember(family, guardian, FamilyRoleType.guardian, FamilyMemberStatus.active);

        var result = await _sut.AddChildAsync(guardian.auth_user_id!.Value, family.family_id, new ChildCreate("Lotte", "Peeters", new DateOnly(2015, 5, 1)));

        Assert.True(result.Succeeded);
        var child = _store.Users.Single(u => u.first_name == "Lotte");
        Assert.Null(child.auth_user_id);
        var membership = _store.Members.Single(m => m.user_id == child.user_id);
        Assert.Equal(FamilyRoleType.child, membership.role_type);
        Assert.Equal(FamilyMemberStatus.active, membership.status);
        Assert.Equal(guardian.user_id, membership.approved_by_user_id);
    }

    [Fact]
    public async Task AddChild_FutureBirthDate_ReturnsValidationFailed()
    {
        var guardian = _store.AddUser();
        var family = _store.AddFamily();
        _store.AddMember(family, guardian, FamilyRoleType.guardian, FamilyMemberStatus.active);

        var result = await _sut.AddChildAsync(guardian.auth_user_id!.Value, family.family_id, new ChildCreate("Lotte", "Peeters", new DateOnly(2030, 1, 1)));

        Assert.Equal(FamilyError.ValidationFailed, result.Error);
    }
    #endregion

    #region Join
    [Fact]
    public async Task Join_ValidCode_CreatesPendingMember()
    {
        var family = _store.AddFamily();
        var invite = AddInvite(family, "ABC234");
        var joiner = _store.AddUser();

        var result = await _sut.JoinFamilyAsync(new JoinFamily("abc 234"), joiner.auth_user_id!.Value);

        Assert.True(result.Succeeded);
        Assert.Equal(FamilyMemberStatus.pending, result.Member!.Status);
        Assert.Equal(1, invite.used_count);
        Assert.Equal(invite.family_invite_id, _store.Members.Single().invite_id);
    }

    [Fact]
    public async Task Join_ExpiredCode_ReturnsInvalidOrExpired()
    {
        var family = _store.AddFamily();
        AddInvite(family, "ABC234");
        var joiner = _store.AddUser();
        _time.Now = _time.Now.AddMinutes(16);

        var result = await _sut.JoinFamilyAsync(new JoinFamily("ABC234"), joiner.auth_user_id!.Value);

        Assert.Equal(FamilyError.InvalidOrExpiredCode, result.Error);
    }

    [Fact]
    public async Task Join_RevokedCode_ReturnsInvalidOrExpired()
    {
        var family = _store.AddFamily();
        AddInvite(family, "ABC234").revoked_at = _time.Now.UtcDateTime;
        var joiner = _store.AddUser();

        var result = await _sut.JoinFamilyAsync(new JoinFamily("ABC234"), joiner.auth_user_id!.Value);

        Assert.Equal(FamilyError.InvalidOrExpiredCode, result.Error);
    }

    [Fact]
    public async Task Join_WrongCode_ReturnsInvalidOrExpired()
    {
        var family = _store.AddFamily();
        AddInvite(family, "ABC234");
        var joiner = _store.AddUser();

        var result = await _sut.JoinFamilyAsync(new JoinFamily("ZZZ999"), joiner.auth_user_id!.Value);

        Assert.Equal(FamilyError.InvalidOrExpiredCode, result.Error);
    }

    [Fact]
    public async Task Join_InvitesDisabled_ReturnsInvitesDisabled()
    {
        var family = _store.AddFamily(inviteEnabled: false);
        AddInvite(family, "ABC234");
        var joiner = _store.AddUser();

        var result = await _sut.JoinFamilyAsync(new JoinFamily("ABC234"), joiner.auth_user_id!.Value);

        Assert.Equal(FamilyError.InvitesDisabled, result.Error);
    }

    [Fact]
    public async Task Join_AlreadyMember_ReturnsAlreadyMember()
    {
        var family = _store.AddFamily();
        AddInvite(family, "ABC234");
        var joiner = _store.AddUser();
        _store.AddMember(family, joiner, FamilyRoleType.guardian, FamilyMemberStatus.active);

        var result = await _sut.JoinFamilyAsync(new JoinFamily("ABC234"), joiner.auth_user_id!.Value);

        Assert.Equal(FamilyError.AlreadyMember, result.Error);
    }

    [Fact]
    public async Task Join_PreviouslyRejected_RequestsAgain()
    {
        var family = _store.AddFamily();
        AddInvite(family, "ABC234");
        var joiner = _store.AddUser();
        var existing = _store.AddMember(family, joiner, FamilyRoleType.guardian, FamilyMemberStatus.rejected);

        var result = await _sut.JoinFamilyAsync(new JoinFamily("ABC234"), joiner.auth_user_id!.Value);

        Assert.True(result.Succeeded);
        Assert.Equal(FamilyMemberStatus.pending, existing.status);
        Assert.Single(_store.Members);
    }
    #endregion

    #region Approve / Reject
    [Fact]
    public async Task Approve_PendingMember_BecomesActive()
    {
        var guardian = _store.AddUser();
        var family = _store.AddFamily();
        _store.AddMember(family, guardian, FamilyRoleType.guardian, FamilyMemberStatus.active);
        var pending = _store.AddMember(family, _store.AddUser(), FamilyRoleType.guardian, FamilyMemberStatus.pending);

        var result = await _sut.ApproveMemberAsync(family.family_id, guardian.auth_user_id!.Value, pending.family_member_id);

        Assert.True(result.Succeeded);
        Assert.Equal(FamilyMemberStatus.active, pending.status);
        Assert.Equal(guardian.user_id, pending.approved_by_user_id);
        Assert.NotNull(pending.approved_at);
    }

    [Fact]
    public async Task Reject_PendingMember_BecomesRejected()
    {
        var guardian = _store.AddUser();
        var family = _store.AddFamily();
        _store.AddMember(family, guardian, FamilyRoleType.guardian, FamilyMemberStatus.active);
        var pending = _store.AddMember(family, _store.AddUser(), FamilyRoleType.guardian, FamilyMemberStatus.pending);

        var result = await _sut.RejectMemberAsync(family.family_id, guardian.auth_user_id!.Value, pending.family_member_id);

        Assert.True(result.Succeeded);
        Assert.Equal(FamilyMemberStatus.rejected, pending.status);
        Assert.Null(pending.approved_at);
    }

    [Fact]
    public async Task Approve_ActiveMember_ReturnsNotPending()
    {
        var guardian = _store.AddUser();
        var family = _store.AddFamily();
        _store.AddMember(family, guardian, FamilyRoleType.guardian, FamilyMemberStatus.active);
        var active = _store.AddMember(family, _store.AddUser(), FamilyRoleType.guardian, FamilyMemberStatus.active);

        var result = await _sut.ApproveMemberAsync(family.family_id, guardian.auth_user_id!.Value, active.family_member_id);

        Assert.Equal(FamilyError.NotPending, result.Error);
    }

    [Fact]
    public async Task Approve_ByPendingUser_ReturnsNotAuthorized()
    {
        var family = _store.AddFamily();
        var pendingUser = _store.AddUser();
        var pending = _store.AddMember(family, pendingUser, FamilyRoleType.guardian, FamilyMemberStatus.pending);

        var result = await _sut.ApproveMemberAsync(family.family_id, pendingUser.auth_user_id!.Value, pending.family_member_id);

        Assert.Equal(FamilyError.NotAuthorized, result.Error);
        Assert.Equal(FamilyMemberStatus.pending, pending.status);
    }

    [Fact]
    public async Task Approve_UnknownMember_ReturnsMemberNotFound()
    {
        var guardian = _store.AddUser();
        var family = _store.AddFamily();
        _store.AddMember(family, guardian, FamilyRoleType.guardian, FamilyMemberStatus.active);

        var result = await _sut.ApproveMemberAsync(family.family_id, guardian.auth_user_id!.Value, 999);

        Assert.Equal(FamilyError.MemberNotFound, result.Error);
    }
    #endregion

    #region Remove
    [Fact]
    public async Task Remove_LastGuardian_ReturnsLastGuardian()
    {
        var guardian = _store.AddUser();
        var family = _store.AddFamily();
        var member = _store.AddMember(family, guardian, FamilyRoleType.guardian, FamilyMemberStatus.active);

        var result = await _sut.RemoveMemberAsync(family.family_id, guardian.auth_user_id!.Value, member.family_member_id);

        Assert.Equal(FamilyError.LastGuardian, result.Error);
        Assert.Equal(FamilyMemberStatus.active, member.status);
    }

    [Fact]
    public async Task Remove_Child_SoftDeletes()
    {
        var guardian = _store.AddUser();
        var family = _store.AddFamily();
        _store.AddMember(family, guardian, FamilyRoleType.guardian, FamilyMemberStatus.active);
        var child = _store.AddMember(family, _store.AddUser(), FamilyRoleType.child, FamilyMemberStatus.active);

        var result = await _sut.RemoveMemberAsync(family.family_id, guardian.auth_user_id!.Value, child.family_member_id);

        Assert.True(result.Succeeded);
        Assert.Equal(FamilyMemberStatus.removed, child.status);
        Assert.Contains(child, _store.Members);
    }

    [Fact]
    public async Task Remove_ChildRemovingOther_ReturnsNotAuthorized()
    {
        var family = _store.AddFamily();
        _store.AddMember(family, _store.AddUser(), FamilyRoleType.guardian, FamilyMemberStatus.active);
        var childUser = _store.AddUser();
        _store.AddMember(family, childUser, FamilyRoleType.child, FamilyMemberStatus.active);
        var other = _store.AddMember(family, _store.AddUser(), FamilyRoleType.child, FamilyMemberStatus.active);

        var result = await _sut.RemoveMemberAsync(family.family_id, childUser.auth_user_id!.Value, other.family_member_id);

        Assert.Equal(FamilyError.NotAuthorized, result.Error);
    }

    [Fact]
    public async Task Remove_SecondGuardianLeaves_Succeeds()
    {
        var family = _store.AddFamily();
        _store.AddMember(family, _store.AddUser(), FamilyRoleType.guardian, FamilyMemberStatus.active);
        var leaverUser = _store.AddUser();
        var leaver = _store.AddMember(family, leaverUser, FamilyRoleType.guardian, FamilyMemberStatus.active);

        var result = await _sut.RemoveMemberAsync(family.family_id, leaverUser.auth_user_id!.Value, leaver.family_member_id);

        Assert.True(result.Succeeded);
        Assert.Equal(FamilyMemberStatus.removed, leaver.status);
    }
    #endregion
}
