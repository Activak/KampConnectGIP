namespace KampConnect.User.Business.Contracts.ClassTypes;

public enum UserError
{
    None,
    UserNotFound,
    UserAlreadyExists
}

public record UserResult(
    bool Succeeded,
    UserError Error = UserError.None,
    Guid? AuthUserId = null,
    UserDto? User = null,
    IDictionary<string, string[]>? ValidationErrors = null)
{
    public static UserResult Ok(Guid? authUserId = null, UserDto? user = null) => new(true, AuthUserId: authUserId, User: user);
    public static UserResult Fail(UserError error) => new(false, error);
}
