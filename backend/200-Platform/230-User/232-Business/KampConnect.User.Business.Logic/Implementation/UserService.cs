using System.ComponentModel;
using KampConnect.User.Business.Contracts.ClassTypes.Results;
using KampConnect.User.Business.Contracts.Interfaces;
using KampConnect.User.Business.Contracts.Requests;
using KampConnect.User.Business.Logic.Mapping;
using KampConnect.User.Data.Interface.Repository;
using MassTransit;
using Microsoft.Extensions.Logging;


namespace KampConnect.User.Business.Logic.Implementation;

using User = KampConnect.User.Data.Entity.User.User;

public class UserService(IUserRepository userRepository, ILogger<UserService> logger, IPublishEndpoint publishEndpoint) : IUserService
{
    #region CreateUserAsync
    public async Task<UserResult> CreateUserAsync(Guid authUserId, UserCreate request, CancellationToken ct = default)
    {
        // Check dat er al een user bestaat met deze authUserId
        if (await userRepository.GetUserByAuthIdAsync(authUserId, ct) is not null)
        {
            return UserResult.Fail(UserError.UserAlreadyExists);
        }

        if (!DateOnly.TryParse(request.BirthDate, out var birthDate))
        {
            return UserResult.Fail(UserError.InvalidBirthDate);
        }

        var now = DateTime.UtcNow;

        // Nieuwe user aanmaken
        var User = new User
        {
            auth_user_id = authUserId,
            first_name = request.FirstName,
            last_name = request.LastName,
            birth_date = birthDate,
            preferred_language = request.PreferredLanguage,
            created_at = now,
            updated_at = now
        };

        // Toevoegen aan repository
        userRepository.Add(User);

        // Opslaan in database
        await userRepository.SaveChangesAsync(ct);

        //TODO: implement event publishing for user creation

        logger.LogInformation("User created for AuthUserId: {AuthUserId}", authUserId);

        // Return de userResult met de nieuw aangemaakte user en authUserId
        return UserResult.Ok(authUserId: authUserId, user: User.ToDto());
    }

    #endregion

    #region GetUserAsync
    public async Task<UserResult> GetUserAsync(Guid authUserId, CancellationToken ct = default)
    {
        var user = await userRepository.GetUserByAuthIdAsync(authUserId, ct);
        if (user is null)
        {
            return UserResult.Fail(UserError.UserNotFound);
        }
        return UserResult.Ok(authUserId: authUserId, user: user.ToDto());
    }
    #endregion
}

