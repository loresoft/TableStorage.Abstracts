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
            key.Should().NotBeNull();

            _output.WriteLine(key);

            string.Compare(key, previousKey).Should().BeLessThan(0);
        }
    }
}
