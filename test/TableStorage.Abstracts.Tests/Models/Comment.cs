namespace TableStorage.Abstracts.Tests.Models;

public class Comment : TableEntityBase
{
    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string OwnerId { get; set; } = null!;
}
