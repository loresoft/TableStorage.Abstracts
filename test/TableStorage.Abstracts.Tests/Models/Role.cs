using System.Runtime.Serialization;
using System.Text.Json;

namespace TableStorage.Abstracts.Tests.Models;

public class Role : TableEntityBase
{
    public string Name { get; set; } = null!;

    public string NormalizedName { get; set; } = null!;

    [IgnoreDataMember]
    public ICollection<Claim> Claims { get; set; } = [];

    [DataMember(Name = "Claims")]
    public string ClaimsJson
    {
        get => JsonSerializer.Serialize(Claims);
        set => Claims = JsonSerializer.Deserialize<List<Claim>>(value) ?? [];
    }
}
