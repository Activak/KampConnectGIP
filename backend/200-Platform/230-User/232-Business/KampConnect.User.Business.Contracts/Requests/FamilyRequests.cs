using KampConnect.User.Data.Entity.Family;

namespace KampConnect.User.Business.Contracts.Requests;

public record FamilyCreate(string Name);

public record FamilyUpdate(string? Name, bool? InviteEnabled);

public record ChildCreate(string FirstName, string LastName, DateOnly BirthDate, string PreferredLanguage = "nl");

public record MemberUpdate(FamilyRoleType? Role, bool? IsPrimaryContact);

public record JoinFamily(string Code);
