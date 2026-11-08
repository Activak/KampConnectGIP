namespace KampConnect.Shared.Infrastructure.Google.Models;

internal sealed record GoogleValidateAddressResponse(GoogleValidationResult? Result);

internal sealed record GoogleValidationResult(GoogleVerdict? Verdict, GoogleAddress? Address);

internal sealed record GoogleVerdict(
    string? ValidationGranularity,
    bool AddressComplete,
    bool HasUnconfirmedComponents);

internal sealed record GoogleAddress(IReadOnlyList<GoogleAddressComponent>? AddressComponents);

internal sealed record GoogleAddressComponent(GoogleComponentName? ComponentName, string? ComponentType);

internal sealed record GoogleComponentName(string? Text);
