using KampConnect.User.Business.Contracts.ClassTypes.Dtos;

namespace KampConnect.User.Business.Logic.Mapping;

using Address = KampConnect.User.Data.Entity.Address.UserAddress;

internal static class AddressMapper
{
    public static AddressDto ToDto(this Address address)
    {
        return new AddressDto(AddressType: address.address_type,
            Street: address.street,
            HouseNumber: address.house_number,
            Box: address.box,
            PostalCode: address.postal_code,
            City: address.city,
            CountryCode: address.country_code,
            IsPrimary: address.is_primary);
    }
}
