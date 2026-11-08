using KampConnect.Shared.Contracts.Authorization;
using KampConnect.User.Business.Contracts.ClassTypes.Results;
using KampConnect.User.Business.Contracts.Interfaces;
using KampConnect.User.Business.Contracts.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KampConnect.User.Service.Controllers;

[ApiController]
[Route("api/user")]
public class UserController(IUserService userService) : ControllerBase
{
    #region GetUser
    [Authorize(Policy = AuthorizationConstants.RequiresAuthenticationPolicy)]
    [HttpGet("me")]
    public async Task<IActionResult> GetUser(CancellationToken ct = default)
    {
        // Check of the user is authenticated and get the authUserId from the claims
        if (!Guid.TryParse(User.FindFirst(KampConnectClaimTypes.Subject)?.Value, out var authUserId))
        {
            return NotFound();
        }

        // User informatie krijgen van de business laag
        UserResult userResult = await userService.GetUserAsync(authUserId, ct);

        // Check of de user niet gevonden is en return NotFound() indien dat het geval is
        if (userResult.Error == UserError.UserNotFound)
        {
            return NotFound();
        }

        // Return 
        return Ok(userResult.User);
    }
    #endregion

    #region CreateUser
    [Authorize(Policy = AuthorizationConstants.RequiresAuthenticationPolicy)]
    [HttpPost("create")]
    public async Task<IActionResult> CreateUser(UserCreate request, CancellationToken ct = default)
    {
        if (!Guid.TryParse(User.FindFirst(KampConnectClaimTypes.Subject)?.Value, out var authUserId))
        {
            return NotFound();
        }

        UserResult userResult = await userService.CreateUserAsync(authUserId, request, ct);

        if (!userResult.Succeeded)
        {
            return Conflict(new { message = userResult.Error });
        }

        return Ok(userResult.User);
    }

    #endregion
}
