using System;
using System.Collections.Generic;
using System.Text;
using KampConnect.User.Business.Contracts.ClassTypes.Dtos;
using KampConnect.User.Business.Contracts.ClassTypes.Results;
using KampConnect.User.Business.Contracts.Requests;

namespace KampConnect.User.Business.Contracts.Interfaces;

public interface IAddressService
{
    public Task<List<AddressDto>?> GetAddressesByUserId(Guid authUserId, CancellationToken ct = default);

    public Task<AddressResult> AddNewAddressByUserId(Guid userId, AddressCreate address, CancellationToken ct = default);
}
