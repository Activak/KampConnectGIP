using KampConnect.Shared.Infrastructure.Contracts.External.AddressValidation.Models;

namespace KampConnect.Shared.Infrastructure.Contracts.External.AddressValidation.Interfaces;

public interface IAddressVerifier
{
    Task<AddressVerificationDto> VerifyAddressAsync(AddressToVerify addressToVerify, CancellationToken ct = default);
}
