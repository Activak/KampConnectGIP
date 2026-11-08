using KampConnect.User.Business.Contracts.ClassTypes.Dtos.Family;
using KampConnect.User.Data.Entity.Family;

namespace KampConnect.User.Business.Logic.Mapping;

using Family = KampConnect.User.Data.Entity.Family.Family;

// Entity -> DTO, zonder gevoelige velden (hash, version)
internal static class FamilyMapper
{
    public static FamilyDto ToDto(this Family family, FamilyRoleType myRole)
    {
        return new FamilyDto(FamilyId: family.family_id,
                             Name: family.name,
                             InviteEnabled: family.invite_enabled,
                             MyRole: myRole);
    }

    public static FamilyMemberDto ToDto(this UserFamilyMember member)
    {
        return new FamilyMemberDto(FamilyMemberId: member.family_member_id,
                                   UserId: member.user_id,
                                   FirstName: member.User?.first_name ?? string.Empty,
                                   LastName: member.User?.last_name ?? string.Empty,
                                   RoleType: member.role_type,
                                   Status: member.status,
                                   IsPrimaryContact: member.is_primary_contact,
                                   RequestedAt: member.requested_at);
    }

    public static FamilyInviteDto ToDto(this UserFamilyInvite invite)
    {
        return new FamilyInviteDto(FamilyInviteId: invite.family_invite_id,
                                   ExpiresAt: invite.expires_at,
                                   UsedCount: invite.used_count ?? 0,
                                   CreatedAt: invite.created_at);
    }
}
