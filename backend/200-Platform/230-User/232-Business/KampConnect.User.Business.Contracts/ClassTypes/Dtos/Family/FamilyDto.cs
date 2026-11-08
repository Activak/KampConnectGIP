using KampConnect.User.Data.Entity.Family;

namespace KampConnect.User.Business.Contracts.ClassTypes.Dtos.Family;

public record FamilyDto(
    Guid FamilyId,
    string Name,
    bool InviteEnabled,
    FamilyRoleType MyRole);
