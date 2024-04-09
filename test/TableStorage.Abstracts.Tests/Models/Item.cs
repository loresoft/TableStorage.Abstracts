namespace TableStorage.Abstracts.Tests.Models;

public class Item : TableEntityBase
{
    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string OwnerId { get; set; } = null!;
}
