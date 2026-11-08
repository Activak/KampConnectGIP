namespace KampConnect.User.Business.Contracts.ClassTypes.Dtos.Family;

public record FamilyInviteDto(
    int FamilyInviteId,
    DateTime ExpiresAt,
    int UsedCount,
    DateTime CreatedAt);
