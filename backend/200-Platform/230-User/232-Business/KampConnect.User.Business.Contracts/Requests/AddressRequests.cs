namespace KampConnect.User.Business.Contracts.Requests;

public record AddressCreate(int AddressType, string Street, string HouseNumber, string Box, string PostalCode, string City, string CountryCode, bool IsPrimary, string LanguageCode = "nl");
