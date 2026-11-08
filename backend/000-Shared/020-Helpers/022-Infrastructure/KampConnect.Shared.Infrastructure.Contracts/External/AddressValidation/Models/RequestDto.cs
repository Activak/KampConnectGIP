using System;
using System.Collections.Generic;
using System.Text;

namespace KampConnect.Shared.Infrastructure.Contracts.External.AddressValidation.Models;

public class RequestDto
{
    public string Street { get; set; } = string.Empty;
    public string HouseNumber { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string CountryCode { get; set; } = string.Empty;
    public string? Box { get; set; }
}
