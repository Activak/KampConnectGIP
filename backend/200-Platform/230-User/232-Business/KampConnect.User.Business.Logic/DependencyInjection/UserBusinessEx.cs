using KampConnect.User.Business.Contracts.Interfaces;
using KampConnect.User.Business.Logic.Implementation;

namespace Microsoft.Extensions.DependencyInjection;

public static class UserBusinessEx
{
    public static IServiceCollection AddUserBusiness(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();

        return services;
    }
}
