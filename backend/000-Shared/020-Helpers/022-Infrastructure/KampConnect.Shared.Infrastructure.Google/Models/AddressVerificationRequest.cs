namespace KampConnect.Shared.Infrastructure.Google.Models;

internal sealed record GoogleValidateAddressRequest(GooglePostalAddress Address);

internal sealed record GooglePostalAddress(
    string RegionCode,
    string PostalCode,
    string Locality,
    IReadOnlyList<string> AddressLines,
    string? LanguageCode);
