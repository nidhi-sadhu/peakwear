using PeakWear.Core.DbModels;
using PeakWear.Core.Models.Account;

namespace PeakWear.Core.Services;

public class AccountService
{
    private readonly IAccountRepository _repository;

    public AccountService(IAccountRepository repository) => _repository = repository;

    public async Task<ProfileResponse?> GetProfileAsync(Guid userId)
    {
        var user = await _repository.GetProfileAsync(userId);
        if (user is null) return null;

        return new ProfileResponse
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            CreatedAtUtc = user.CreatedAtUtc,
            Preference = user.Preference is null ? null : Map(user.Preference),
            Addresses = user.Addresses.Select(Map).ToList()
        };
    }

    public async Task<ProfileResponse?> UpdateProfileAsync(Guid userId, UpdateProfileRequest request)
    {
        var user = await _repository.GetForUpdateAsync(userId);
        if (user is null) return null;

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.PhoneNumber = request.PhoneNumber;
        user.UpdatedAtUtc = DateTime.UtcNow;

        await _repository.SaveAsync();
        return await GetProfileAsync(userId);
    }

    public async Task<PreferenceResponse> UpsertPreferenceAsync(
        Guid userId, UpdatePreferenceRequest request)
    {
        var preference = await _repository.GetPreferenceAsync(userId);

        if (preference is null)
        {
            preference = new UserPreference
            {
                UserId = userId,
                ShoppingFor = request.ShoppingFor,
                SizeTop = request.SizeTop,
                SizeBottom = request.SizeBottom,
                MarketingOptIn = request.MarketingOptIn
            };
            await _repository.AddPreferenceAsync(preference);
        }
        else
        {
            preference.ShoppingFor = request.ShoppingFor;
            preference.SizeTop = request.SizeTop;
            preference.SizeBottom = request.SizeBottom;
            preference.MarketingOptIn = request.MarketingOptIn;
            preference.UpdatedAtUtc = DateTime.UtcNow;
            await _repository.SaveAsync();
        }

        return Map(preference);
    }

    public async Task<List<AddressResponse>> GetAddressesAsync(Guid userId) =>
        (await _repository.GetAddressesAsync(userId)).Select(Map).ToList();

    public async Task<List<AddressResponse>> AddAddressAsync(Guid userId, AddressRequest request)
    {
        var existing = await _repository.GetAddressesAsync(userId);

        // First address is automatically the default
        var makeDefault = request.IsDefault || existing.Count == 0;

        if (makeDefault)
            await _repository.ClearDefaultAddressesAsync(userId);

        await _repository.AddAddressAsync(new Address
        {
            UserId = userId,
            Line1 = request.Line1,
            Line2 = request.Line2,
            City = request.City,
            State = request.State,
            PostalCode = request.PostalCode,
            CountryCode = request.CountryCode.ToUpper(),
            IsDefault = makeDefault
        });

        return await GetAddressesAsync(userId);
    }

    public async Task<List<AddressResponse>?> UpdateAddressAsync(
        Guid userId, Guid addressId, AddressRequest request)
    {
        var address = await _repository.GetAddressAsync(userId, addressId);
        if (address is null) return null;

        if (request.IsDefault && !address.IsDefault)
        {
            await _repository.ClearDefaultAddressesAsync(userId);
            address = await _repository.GetAddressAsync(userId, addressId);
            if (address is null) return null;
        }

        address.Line1 = request.Line1;
        address.Line2 = request.Line2;
        address.City = request.City;
        address.State = request.State;
        address.PostalCode = request.PostalCode;
        address.CountryCode = request.CountryCode.ToUpper();
        address.IsDefault = request.IsDefault || address.IsDefault;
        address.UpdatedAtUtc = DateTime.UtcNow;

        await _repository.SaveAsync();
        return await GetAddressesAsync(userId);
    }

    public async Task<List<AddressResponse>?> DeleteAddressAsync(Guid userId, Guid addressId)
    {
        var address = await _repository.GetAddressAsync(userId, addressId);
        if (address is null) return null;

        var wasDefault = address.IsDefault;
        await _repository.RemoveAddressAsync(address);

        // Promote another address to default so the user always has one
        if (wasDefault)
        {
            var remaining = await _repository.GetAddressesAsync(userId);
            var next = remaining.FirstOrDefault();
            if (next is not null)
            {
                var tracked = await _repository.GetAddressAsync(userId, next.Id);
                if (tracked is not null)
                {
                    tracked.IsDefault = true;
                    await _repository.SaveAsync();
                }
            }
        }

        return await GetAddressesAsync(userId);
    }

    public async Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordRequest request)
    {
        var user = await _repository.GetForUpdateAsync(userId);
        if (user is null) return false;

        // Always verify the current password — a stolen session shouldn't be
        // enough to lock the real owner out of their account.
        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            return false;

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.UpdatedAtUtc = DateTime.UtcNow;
        await _repository.SaveAsync();
        return true;
    }

    private static PreferenceResponse Map(UserPreference p) => new()
    {
        ShoppingFor = p.ShoppingFor,
        SizeTop = p.SizeTop,
        SizeBottom = p.SizeBottom,
        MarketingOptIn = p.MarketingOptIn
    };

    private static AddressResponse Map(Address a) => new()
    {
        Id = a.Id,
        Line1 = a.Line1,
        Line2 = a.Line2,
        City = a.City,
        State = a.State,
        PostalCode = a.PostalCode,
        CountryCode = a.CountryCode,
        IsDefault = a.IsDefault
    };
}