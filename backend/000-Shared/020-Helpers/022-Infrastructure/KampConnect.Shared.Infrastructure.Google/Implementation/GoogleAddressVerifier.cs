using System.Net.Http.Json;
using KampConnect.Shared.Infrastructure.Contracts.External.AddressValidation.Interfaces;
using KampConnect.Shared.Infrastructure.Contracts.External.AddressValidation.Models;
using KampConnect.Shared.Infrastructure.Google.Models;

namespace KampConnect.Shared.Infrastructure.Google.Implementation;

internal sealed class GoogleAddressVerifier(HttpClient httpClient) : IAddressVerifier
{
    // "./" is nodig, anders ziet Uri "v1:" als scheme
    private const string ValidateEndpoint = "./v1:validateAddress";

    // Enkel adressen bevestigd tot op gebouw- of busniveau zijn geldig
    private static readonly HashSet<string> AcceptedGranularities = ["PREMISE", "SUB_PREMISE"];

    public async Task<AddressVerificationDto> VerifyAddressAsync(AddressToVerify addressToVerify, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(addressToVerify);

        var countryCode = addressToVerify.CountryCode.ToUpperInvariant();

        var request = new GoogleValidateAddressRequest(
            new GooglePostalAddress(
                RegionCode: countryCode,
                PostalCode: addressToVerify.PostalCode,
                Locality: addressToVerify.City,
                AddressLines: [BuildAddressLine(addressToVerify)],
                LanguageCode: addressToVerify.LanguageCode));

        using var response = await httpClient.PostAsJsonAsync(ValidateEndpoint, request, ct);
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<GoogleValidateAddressResponse>(ct);
        var verdict = body?.Result?.Verdict;

        if (verdict is null)
        {
            return new AddressVerificationDto(false, null, null, null, null, null, null);
        }

        var isValid = verdict.AddressComplete
            && !verdict.HasUnconfirmedComponents
            && verdict.ValidationGranularity is not null
            && AcceptedGranularities.Contains(verdict.ValidationGranularity);

        var components = body!.Result!.Address?.AddressComponents ?? [];

        return new AddressVerificationDto(
            IsValid: isValid,
            NormalizedStreet: GetComponent(components, "route"),
            NormalizedHouseNumber: GetComponent(components, "street_number"),
            NormalizedBox: GetComponent(components, "subpremise"),
            NormalizedPostalCode: GetComponent(components, "postal_code"),
            NormalizedCity: GetComponent(components, "locality"),
            NormalizedCountryCode: countryCode);
    }

    // Bv. "Veldstraat 1" of "Veldstraat 1 bus 3"
    private static string BuildAddressLine(AddressToVerify address) =>
        string.IsNullOrWhiteSpace(address.Box)
            ? $"{address.Street} {address.HouseNumber}"
            : $"{address.Street} {address.HouseNumber} bus {address.Box}";

    private static string? GetComponent(IReadOnlyList<GoogleAddressComponent> components, string type) =>
        components.FirstOrDefault(c => c.ComponentType == type)?.ComponentName?.Text;
}
