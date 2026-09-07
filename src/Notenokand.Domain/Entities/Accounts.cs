using Notenokand.Domain.Common;

namespace Notenokand.Domain.Entities;

public sealed class Account : Entity
{
    public required string Name { get; set; }
    public string? BusinessType { get; set; }
    public string? TaxId { get; set; }
    public string? AddressLine { get; set; }
    public short? ProvinceCode { get; set; }
    public int? DistrictCode { get; set; }
    public int? SubdistrictCode { get; set; }
    public string? PostalCode { get; set; }
    public string TimeZoneId { get; set; } = "Asia/Bangkok";
    public string CurrencyCode { get; set; } = "THB";
    public bool IsActive { get; set; } = true;
    public ICollection<AccountUser> Members { get; set; } = [];
}

public sealed class AccountUser : Entity
{
    public Guid AccountId { get; set; }
    public Guid UserId { get; set; }
    public required string RoleName { get; set; }
    public bool IsActive { get; set; } = true;
    public Account Account { get; set; } = null!;
}

public sealed class AccountInvitation : Entity
{
    public Guid AccountId { get; set; }
    public required string Email { get; set; }
    public required string RoleName { get; set; }
    public required string TokenHash { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? AcceptedAt { get; set; }
}

public sealed class UserConsent : Entity
{
    public Guid UserId { get; set; }
    public required string ConsentType { get; set; }
    public required string Version { get; set; }
    public DateTimeOffset AcceptedAt { get; set; }
    public string? IpAddress { get; set; }
}

public sealed class EmailVerificationLog : Entity
{
    public Guid UserId { get; set; }
    public DateTimeOffset RequestedAt { get; set; }
    public DateTimeOffset? ConfirmedAt { get; set; }
    public string? IpAddress { get; set; }
}