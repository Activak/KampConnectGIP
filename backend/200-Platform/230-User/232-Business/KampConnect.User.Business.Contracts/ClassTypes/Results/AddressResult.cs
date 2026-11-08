using System;
using System.Collections.Generic;
using System.Text;
using KampConnect.User.Business.Contracts.ClassTypes.Dtos;

namespace KampConnect.User.Business.Contracts.ClassTypes.Results;

public enum AddressError
{
    None,
    AddressNotFound,
    AddressAlreadyExists,
    InvalidAddress,
    UserNotFound
}

public record AddressResult(
    bool Succeeded,
    AddressError Error = AddressError.None,
    Guid? UserId = null,
    AddressDto? Address = null,
    string? VerificationMessage = "",
    IDictionary<string, string[]>? ValidationErrors = null)
{
    public static AddressResult Ok(Guid? userId = null, AddressDto? address = null, string? verificationMessage = "") => new(true, UserId: userId, Address: address, VerificationMessage: verificationMessage);
    public static AddressResult Fail(AddressError error) => new(false, error);
}
