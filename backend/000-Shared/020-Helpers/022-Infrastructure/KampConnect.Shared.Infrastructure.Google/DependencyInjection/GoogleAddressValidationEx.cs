

using KampConnect.Shared.Infrastructure.Contracts.External.AddressValidation.Interfaces;
using KampConnect.Shared.Infrastructure.Google.Configuration;
using KampConnect.Shared.Infrastructure.Google.Implementation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection;

public static class GoogleAddressValidationEx
{
    public static IServiceCollection AddGoogleAddressValidation(this IServiceCollection services, IConfiguration configuration)
    {
        // Faalt bij opstarten als de API-key ontbreekt
        services.AddOptions<GoogleMapsOptions>()
            .Bind(configuration.GetSection(GoogleMapsOptions.SectionName))
            .Validate(o => !string.IsNullOrWhiteSpace(o.ApiKey), "GoogleMaps:ApiKey is niet ingesteld.")
            .ValidateOnStart();

        services.AddHttpClient<IAddressVerifier, GoogleAddressVerifier>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<GoogleMapsOptions>>().Value;

            client.BaseAddress = new Uri(options.BaseUrl);
            // Key in header i.p.v. URL, zodat hij niet in logs belandt
            client.DefaultRequestHeaders.Add("X-Goog-Api-Key", options.ApiKey);
            client.Timeout = TimeSpan.FromSeconds(10);
        });

        return services;
    }
}
