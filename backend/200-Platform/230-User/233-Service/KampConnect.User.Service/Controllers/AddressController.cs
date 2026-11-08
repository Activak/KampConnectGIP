using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using KampConnect.User.Business.Contracts.Interfaces;
using KampConnect.Shared.Contracts.Authorization;
using KampConnect.User.Business.Contracts.Requests;

namespace KampConnect.User.Service.Controllers;

[ApiController]
[Route("api/address")]
public class AddressController(IAddressService addressService, IUserService userService) : ControllerBase
{

    #region GetAddressesByUserId
    [Authorize(Policy = AuthorizationConstants.RequiresAuthenticationPolicy)]
    [HttpGet("me")]
    public async Task<IActionResult> GetAddressesByUserId(CancellationToken ct = default)
    { 
        if (!Guid.TryParse(User.FindFirst(KampConnectClaimTypes.Subject)?.Value, out var authUserId))
        {
            return NotFound();
        }
        var addresses = await addressService.GetAddressesByUserId(authUserId, ct);
        if (addresses is null)
        {
            return NotFound();
        }

        return Ok(addresses);
    }
    #endregion

    #region AddNewAddressByUserId
    [Authorize(Policy = AuthorizationConstants.RequiresAuthenticationPolicy)]
    [HttpPost("me")]
    public async Task<IActionResult> AddNewAddressByUserId([FromBody] AddressCreate address, CancellationToken ct = default)
    {
        if (!Guid.TryParse(User.FindFirst(KampConnectClaimTypes.Subject)?.Value, out var authUserId))
        {
            return NotFound();
        }

        var addressResult = await addressService.AddNewAddressByUserId(authUserId, address, ct);
        if (!addressResult.Succeeded)
        {
            return Conflict(new { message = addressResult.Error });
        }
        return Ok(addressResult);
    }
    #endregion
}
