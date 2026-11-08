using KampConnect.User.Data.Entity.Address;

namespace KampConnect.User.Business.Contracts.ClassTypes.Dtos;

public record AddressDto(
    AddressType AddressType,
    string? Street,
    string? HouseNumber,
    string? Box,
    string? PostalCode,
    string? City,
    string? CountryCode,
    bool IsPrimary
);
