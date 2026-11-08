using System;
using System.Collections.Generic;
using System.Text;

namespace KampConnect.Shared.Infrastructure.Contracts.External.AddressValidation.Models;

public record AddressVerificationDto(
    bool IsValid,
    string? NormalizedStreet,
    string? NormalizedHouseNumber,
    string? NormalizedBox,
    string? NormalizedPostalCode,
    string? NormalizedCity,
    string? NormalizedCountryCode
    );
