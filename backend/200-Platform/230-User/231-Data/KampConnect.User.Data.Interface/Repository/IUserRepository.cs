namespace KampConnect.User.Data.Interface.Repository;

using User = KampConnect.User.Data.Entity.User.User;

public interface IUserRepository
{
    public Task<User?> GetUserByAuthIdAsync(Guid authUserId, CancellationToken ct = default);

    public void Add(User user);

    public Task SaveChangesAsync(CancellationToken ct = default);
}

