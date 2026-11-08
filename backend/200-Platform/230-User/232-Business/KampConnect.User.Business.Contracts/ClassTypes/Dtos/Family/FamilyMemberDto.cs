using KampConnect.User.Data.Entity.Family;

namespace KampConnect.User.Business.Contracts.ClassTypes.Dtos.Family;

public record FamilyMemberDto(
    int FamilyMemberId,
    Guid UserId,
    string FirstName,
    string LastName,
    FamilyRoleType RoleType,
    FamilyMemberStatus Status,
    bool IsPrimaryContact,
    DateTime? RequestedAt);
