namespace KampConnect.User.Business.Contracts.ClassTypes.Dtos.Family;

public record CreatedInviteDto(
    int FamilyInviteId,
    string Code,
    DateTime ExpiresAt);
