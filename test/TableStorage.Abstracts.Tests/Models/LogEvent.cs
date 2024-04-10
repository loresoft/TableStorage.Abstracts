namespace TableStorage.Abstracts.Tests.Models;

public class LogEvent : TableEntityBase
{
    public string? Level { get; set; }

    public string? MessageTemplate { get; set; }

    public string? RenderedMessage { get; set; }

    public string? Exception { get; set; }

    public string? Data { get; set; }
}
