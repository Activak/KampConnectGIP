using KampConnect.User.Data.Context.Implentation;
using KampConnect.User.Data.Interface.Repository;
using Microsoft.EntityFrameworkCore;

namespace KampConnect.User.Data.Repository.Implentation;

using User = KampConnect.User.Data.Entity.User.User;

public class UserRepository(KampConnectUserDbContext dbContext) : IUserRepository
{
    public void Add(User user) => dbContext.Users.Add(user);

    public Task<User?> GetUserByAuthIdAsync(Guid authUserId, CancellationToken ct = default) =>
        dbContext.Users.FirstOrDefaultAsync(u => u.auth_user_id == authUserId, ct);

    public Task SaveChangesAsync(CancellationToken ct = default) => dbContext.SaveChangesAsync(ct);
}

