using KampConnect.User.Data.Entity.User;

namespace KampConnect.User.Business.Contracts.ClassTypes;

public record UserDto(
    Guid UserId,
    Guid AuthUserId,
    string FirstName,
    string LastName,
    string BirthDate,
    string PreferredLanguage,
    UserStatus Status
);
