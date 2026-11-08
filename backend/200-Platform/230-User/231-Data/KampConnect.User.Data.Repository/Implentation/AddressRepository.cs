using KampConnect.User.Data.Context.Implentation;
using KampConnect.User.Data.Entity.Address;
using KampConnect.User.Data.Interface.Repository;
using Microsoft.EntityFrameworkCore;

namespace KampConnect.User.Data.Repository.Implentation;

public class AddressRepository(KampConnectUserDbContext dbContext) : IAddressRepository
{
    public void Add(UserAddress address) => dbContext.UserAddress.Add(address);

    public async Task<List<UserAddress>> GetAddressIdsByUserIdAsync(Guid userId, CancellationToken ct = default) =>
        await dbContext.UserAddress.Where(a => a.user_id == userId).ToListAsync(ct);
}
