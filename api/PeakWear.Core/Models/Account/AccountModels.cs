using System.ComponentModel.DataAnnotations;

namespace PeakWear.Core.Models.Account;

public class ProfileResponse
{
    public Guid Id { get; set; }
    public string Email { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string? PhoneNumber { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public PreferenceResponse? Preference { get; set; }
    public List<AddressResponse> Addresses { get; set; } = [];
}

public class UpdateProfileRequest
{
    [Required, StringLength(64)]
    public string FirstName { get; set; } = "";

    [Required, StringLength(64)]
    public string LastName { get; set; } = "";

    [StringLength(20)]
    public string? PhoneNumber { get; set; }
}

public class PreferenceResponse
{
    public string ShoppingFor { get; set; } = "Both";
    public string? SizeTop { get; set; }
    public string? SizeBottom { get; set; }
    public bool MarketingOptIn { get; set; }
}

public class UpdatePreferenceRequest
{
    [Required, StringLength(16)]
    public string ShoppingFor { get; set; } = "Both";

    [StringLength(8)]
    public string? SizeTop { get; set; }

    [StringLength(8)]
    public string? SizeBottom { get; set; }

    public bool MarketingOptIn { get; set; }
}

public class AddressResponse
{
    public Guid Id { get; set; }
    public string Line1 { get; set; } = "";
    public string? Line2 { get; set; }
    public string City { get; set; } = "";
    public string State { get; set; } = "";
    public string PostalCode { get; set; } = "";
    public string CountryCode { get; set; } = "US";
    public bool IsDefault { get; set; }
}

public class AddressRequest
{
    [Required, StringLength(100)]
    public string Line1 { get; set; } = "";

    [StringLength(100)]
    public string? Line2 { get; set; }

    [Required, StringLength(64)]
    public string City { get; set; } = "";

    [Required, StringLength(64)]
    public string State { get; set; } = "";

    [Required, StringLength(16)]
    public string PostalCode { get; set; } = "";

    [Required, StringLength(2)]
    public string CountryCode { get; set; } = "US";

    public bool IsDefault { get; set; }
}

public class ChangePasswordRequest
{
    [Required]
    public string CurrentPassword { get; set; } = "";

    [Required, StringLength(100, MinimumLength = 8)]
    public string NewPassword { get; set; } = "";
}