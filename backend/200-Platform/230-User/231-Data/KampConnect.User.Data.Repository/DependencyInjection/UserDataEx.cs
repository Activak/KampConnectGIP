using KampConnect.User.Data.Interface.Repository;
using KampConnect.User.Data.Repository.Implentation;


namespace Microsoft.Extensions.DependencyInjection;

public static class UserDataEx
{
    public static IServiceCollection AddUserData(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IAddressRepository, AddressRepository>();
        services.AddScoped<IFamilyRepository, FamilyRepository>();
        services.AddScoped<IFamilyInviteRepository, FamilyInviteRepository>();

        return services;
    }
}

