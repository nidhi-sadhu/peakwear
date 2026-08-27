using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeakWear.Core.Models.Account;
using PeakWear.Core.Services;

namespace PeakWear.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AccountController : ControllerBase
{
    private readonly AccountService _accountService;

    public AccountController(AccountService accountService) => _accountService = accountService;

    private Guid UserId =>
        Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("sub")!.Value);

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var profile = await _accountService.GetProfileAsync(UserId);
        return profile is null ? NotFound() : Ok(profile);
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile(UpdateProfileRequest request)
    {
        var profile = await _accountService.UpdateProfileAsync(UserId, request);
        return profile is null ? NotFound() : Ok(profile);
    }

    [HttpPut("preferences")]
    public async Task<IActionResult> UpdatePreferences(UpdatePreferenceRequest request) =>
        Ok(await _accountService.UpsertPreferenceAsync(UserId, request));

    [HttpGet("addresses")]
    public async Task<IActionResult> GetAddresses() =>
        Ok(await _accountService.GetAddressesAsync(UserId));

    [HttpPost("addresses")]
    public async Task<IActionResult> AddAddress(AddressRequest request) =>
        Ok(await _accountService.AddAddressAsync(UserId, request));

    [HttpPut("addresses/{addressId:guid}")]
    public async Task<IActionResult> UpdateAddress(Guid addressId, AddressRequest request)
    {
        var addresses = await _accountService.UpdateAddressAsync(UserId, addressId, request);
        return addresses is null ? NotFound() : Ok(addresses);
    }

    [HttpDelete("addresses/{addressId:guid}")]
    public async Task<IActionResult> DeleteAddress(Guid addressId)
    {
        var addresses = await _accountService.DeleteAddressAsync(UserId, addressId);
        return addresses is null ? NotFound() : Ok(addresses);
    }

    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
    {
        var success = await _accountService.ChangePasswordAsync(UserId, request);
        return success
            ? Ok(new { message = "Password updated." })
            : BadRequest(new { message = "Your current password isn't correct." });
    }
}