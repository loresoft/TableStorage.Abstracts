namespace TableStorage.Abstracts.Tests.Models;

public class Login
{
    public string LoginProvider { get; set; } = null!;

    public string ProviderKey { get; set; } = null!;

    public string? DisplayName { get; set; }
}
