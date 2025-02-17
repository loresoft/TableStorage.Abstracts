using TableStorage.Abstracts.Extensions;

namespace TableStorage.Abstracts.Tests;

public class UlidGeneratorTests
{
    private readonly ITestOutputHelper _output;

    public UlidGeneratorTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void UlidTests()
    {
        var timestamp = DateTimeOffset.UtcNow.ToReverseChronological();

        var previousKey = Ulid.NewUlid(timestamp).ToString();

        // higher dates should be lower sort
        for (int i = 0; i < 100; i++)
        {
            timestamp = DateTimeOffset.UtcNow.ToReverseChronological();
            var key = Ulid.NewUlid(timestamp).ToString();
            Assert.NotNull(key);

            _output.WriteLine(key);

            Assert.True((string.Compare(key, previousKey)) < (0));
        }
    }
}
