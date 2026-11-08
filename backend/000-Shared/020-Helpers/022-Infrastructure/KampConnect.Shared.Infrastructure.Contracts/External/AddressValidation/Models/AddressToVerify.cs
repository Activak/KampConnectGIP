using System;
using System.Collections.Generic;
using System.Text;

namespace KampConnect.Shared.Infrastructure.Contracts.External.AddressValidation.Models;

public record AddressToVerify(
    string Street,
    string HouseNumber,
    string PostalCode,
    string City,
    string CountryCode,
    string? Box = null,
    string? LanguageCode = null
);
