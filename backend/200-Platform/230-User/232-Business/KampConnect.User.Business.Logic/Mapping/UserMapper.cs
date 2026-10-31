
using KampConnect.User.Business.Contracts.ClassTypes;

namespace KampConnect.User.Business.Logic.Mapping;

using User = KampConnect.User.Data.Entity.User.User;

internal static class UserMapper
{

    public static UserDto ToDto(this User user)
    {
        return new UserDto( UserId: user.user_id,
                            AuthUserId: user.auth_user_id,
                            FirstName: user.first_name,
                            LastName: user.last_name,
                            BirthDate: user.birth_date.ToString("yyyy-MM-dd"),
                            PreferredLanguage: user.preferred_language,
                            Status: user.status);
    }
}
