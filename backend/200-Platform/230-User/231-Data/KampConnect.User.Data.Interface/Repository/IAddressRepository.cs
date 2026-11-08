using KampConnect.User.Data.Entity.Address;

namespace KampConnect.User.Data.Interface.Repository;

public interface IAddressRepository
{
    public Task<List<UserAddress>> GetAddressIdsByUserIdAsync(Guid userId, CancellationToken ct = default);

    public void Add(UserAddress address);
}
