using System.ComponentModel;

namespace TableStorage.Abstracts.Tests.Models;

public class User : TableEntityBase
{
    public User()
    {
        Claims = [];
        Logins = [];
        Tokens = [];
        Roles = [];
        Organizations = [];
    }

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    [DefaultValue(false)]
    public bool EmailConfirmed { get; set; }


    public string? PasswordHash { get; set; }

    public string? SecurityStamp { get; set; }


    public string? PhoneNumber { get; set; }

    [DefaultValue(false)]
    public bool PhoneNumberConfirmed { get; set; }


    [DefaultValue(false)]
    public bool TwoFactorEnabled { get; set; }


    public DateTimeOffset? LockoutEndDateUtc { get; set; }

    [DefaultValue(false)]
    public bool LockoutEnabled { get; set; }

    [DefaultValue(0)]
    public int AccessFailedCount { get; set; }


    public HashSet<string> Roles { get; set; }

    public ICollection<Claim> Claims { get; set; }

    public ICollection<Login> Logins { get; set; }

    public ICollection<Token> Tokens { get; set; }

    public HashSet<string> Organizations { get; set; }
}
