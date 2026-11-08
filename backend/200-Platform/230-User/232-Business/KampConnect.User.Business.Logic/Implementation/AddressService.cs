using KampConnect.Shared.Infrastructure.Contracts.External.AddressValidation.Interfaces;
using KampConnect.Shared.Infrastructure.Contracts.External.AddressValidation.Models;
using KampConnect.User.Business.Contracts.ClassTypes.Dtos;
using KampConnect.User.Business.Contracts.ClassTypes.Results;
using KampConnect.User.Business.Contracts.Interfaces;
using KampConnect.User.Business.Contracts.Requests;
using KampConnect.User.Business.Logic.Mapping;
using KampConnect.User.Data.Entity.Address;
using KampConnect.User.Data.Interface.Repository;
using Microsoft.Extensions.Logging;

namespace KampConnect.User.Business.Logic.Implementation;

public class AddressService(IAddressRepository addressRepository,
                            IUserRepository userRepository,
                            IAddressVerifier addressVerifier,
                            ILogger<AddressService> logger) : IAddressService
{
    public async Task<AddressResult> AddNewAddressByUserId(Guid authUserId, AddressCreate address, CancellationToken ct = default)
    {
        // UserId ophalen obv authUserId
        var user = await userRepository.GetUserByAuthIdAsync(authUserId, ct);

        if (user is null)
        {
            return AddressResult.Fail(AddressError.UserNotFound);
        }

        var userId = user!.user_id;

        var now = DateTime.UtcNow;

        // Address intialiseren
        var Address = new UserAddress
        {
            user_id = userId,
            address_type = (AddressType)address.AddressType,
            street = address.Street,
            house_number = address.HouseNumber,
            box = address.Box,
            postal_code = address.PostalCode,
            city = address.City,
            country_code = address.CountryCode,
            is_primary = address.IsPrimary,
            is_validated = false,
            created_at = now,
            updated_at = now
        };


        // Check of het  adres echt bestaat via de google api
        string FailedVerificationMessage = string.Empty;

        var addressToVerify = new AddressToVerify(
            Street: address.Street,
            HouseNumber: address.HouseNumber,
            PostalCode: address.PostalCode,
            City: address.City,
            CountryCode: address.CountryCode,
            Box: address.Box,
            LanguageCode: address.LanguageCode
        );

        try
        {
            var verificationResult = await addressVerifier.VerifyAddressAsync(addressToVerify, ct);

            if (verificationResult.IsValid)
            {
                // Return normalized values
                Address = new UserAddress
                {
                    user_id = userId,
                    address_type = (AddressType)address.AddressType,
                    // Straat en stad blijven in de taal van de gebruiker
                    street = address.Street,
                    house_number = verificationResult.NormalizedHouseNumber ?? address.HouseNumber,
                    box = verificationResult.NormalizedBox ?? address.Box,
                    postal_code = verificationResult.NormalizedPostalCode ?? address.PostalCode,
                    city = address.City,
                    country_code = verificationResult.NormalizedCountryCode ?? address.CountryCode,
                    is_primary = address.IsPrimary,
                    is_validated = verificationResult.IsValid,
                    created_at = now,
                    updated_at = now
                };  
            }
            else
            {
                FailedVerificationMessage = "Address verification failed.";
            }
        }
        catch (Exception ex) when (ex is HttpRequestException || ex is TaskCanceledException && !ct.IsCancellationRequested)
        {
            logger.LogWarning(ex, "Address verification failed for user {UserId}.", userId);
            FailedVerificationMessage = "Address verification failed due to an unexpected error.";
        }

        addressRepository.Add(Address);

        await userRepository.SaveChangesAsync(ct);


        return AddressResult.Ok(userId: userId, address: Address.ToDto(), verificationMessage: Address.is_validated ? null : FailedVerificationMessage);
    }

    public async Task<List<AddressDto>?> GetAddressesByUserId(Guid authUserId, CancellationToken ct = default)
    {
        var user = await userRepository.GetUserByAuthIdAsync(authUserId, ct);
        if (user is null)
        {
            return null;
        }

        var addresses = await addressRepository.GetAddressIdsByUserIdAsync(user.user_id, ct);
        return addresses.Select(a => a.ToDto()).ToList();
    }
}
