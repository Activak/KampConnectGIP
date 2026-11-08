using KampConnect.User.Business.Contracts.Interfaces;
using KampConnect.User.Business.Logic.Implementation;

namespace Microsoft.Extensions.DependencyInjection;

public static class UserBusinessEx
{
    public static IServiceCollection AddUserBusiness(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAddressService, AddressService>();
        services.AddScoped<IFamilyService, FamilyService>();
        services.AddScoped<IFamilyInviteService, FamilyInviteService>();
        services.AddSingleton(TimeProvider.System);

        return services;
    }
}
