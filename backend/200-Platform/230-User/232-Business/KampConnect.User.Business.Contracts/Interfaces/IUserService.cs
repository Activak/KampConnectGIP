using KampConnect.User.Business.Contracts.ClassTypes;
using KampConnect.User.Business.Contracts.Requests;

namespace KampConnect.User.Business.Contracts.Interfaces;

public interface IUserService
{
    public Task<UserResult> GetUserAsync(Guid authUserId, CancellationToken ct = default);

    public Task<UserResult> CreateUserAsync(Guid authUserId, UserCreate request, CancellationToken ct = default);
}
