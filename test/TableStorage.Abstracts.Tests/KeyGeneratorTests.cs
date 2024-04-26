using TableStorage.Abstracts.Extensions;

namespace TableStorage.Abstracts.Tests;

public class KeyGeneratorTests
{
    private readonly ITestOutputHelper _output;

    public KeyGeneratorTests(ITestOutputHelper output)
    {
        _output = output;
    }


    [Fact]
    public void GenerateRowKeyDateTimeOffsetNow()
    {
        var dateTime = new DateTimeOffset(2024, 4, 1, 23, 0, 0, TimeSpan.FromHours(-5));

        var rowKey = KeyGenerator.GenerateRowKey(dateTime);
        rowKey.Should().NotBeNull();

        var parsed = Ulid.TryParse(rowKey, out var ulid);
        parsed.Should().BeTrue();
        ulid.Should().NotBeNull();

        var reversed = dateTime.ToUniversalTime().ToReverseChronological();
        var ulidDate = ulid.Time;

        ulidDate.Year.Should().Be(reversed.Year);
        ulidDate.Month.Should().Be(reversed.Month);
        ulidDate.Day.Should().Be(reversed.Day);
        ulidDate.Hour.Should().Be(reversed.Hour);
        ulidDate.Minute.Should().Be(reversed.Minute);
    }


    [Fact]
    public void GeneratePartitionKeyDateTimeOffsetNow()
    {
        var dateTime = new DateTimeOffset(2024, 4, 1, 23, 0, 0, TimeSpan.FromHours(-5));

        var partitionKey = KeyGenerator.GeneratePartitionKey(dateTime);
        partitionKey.Should().NotBeNull();
        partitionKey.Should().Be("2516902703999999999");
    }

    [Fact]
    public void GeneratePartitionKeyDateTimeNow()
    {
        var dateTime = new DateTimeOffset(2024, 4, 1, 23, 0, 0, TimeSpan.FromHours(-5));
        var eventTime = dateTime.UtcDateTime;

        var partitionKey = KeyGenerator.GeneratePartitionKey(eventTime);
        partitionKey.Should().NotBeNull();
        partitionKey.Should().Be("2516902703999999999");
    }

    [Theory]
    [MemberData(nameof(GetDateRounding))]
    public void GeneratePartitionKeyDateTimeNowRound(DateTimeOffset dateTime, string expected)
    {
        var partitionKey = KeyGenerator.GeneratePartitionKey(dateTime);
        partitionKey.Should().NotBeNull();
        partitionKey.Should().Be(expected);
    }

    public static IEnumerable<object[]> GetDateRounding()
    {
        yield return new object[]
        {
            new DateTimeOffset(2024, 4, 1, 23, 1, 0, TimeSpan.FromHours(-5)),
            "2516902703999999999"
        };
        yield return new object[]
        {
            new DateTimeOffset(2024, 4, 1, 23, 2, 55, TimeSpan.FromHours(-5)),
            "2516902700999999999"
        };
        yield return new object[]
        {
            new DateTimeOffset(2024, 4, 1, 23, 3, 5, TimeSpan.FromHours(-5)),
            "2516902700999999999"
        };
        yield return new object[]
        {
            new DateTimeOffset(2024, 4, 1, 23, 4, 11, TimeSpan.FromHours(-5)),
            "2516902700999999999"
        };
        yield return new object[]
        {
            new DateTimeOffset(2024, 4, 1, 23, 4, 43, TimeSpan.FromHours(-5)),
            "2516902700999999999"
        };
    }


    [Fact]
    public void GeneratePartitionKeyQueryDateOnly()
    {
        var date = new DateOnly(2024, 4, 1);

        var partitionKeyQuery = KeyGenerator.GeneratePartitionKeyQuery(date, TimeSpan.FromHours(-5));
        partitionKeyQuery.Should().NotBeNull();
        partitionKeyQuery.Should().Be("(PartitionKey ge '2516902667999999999') and (PartitionKey lt '2516903531999999999')");
    }

    [Fact]
    public void GeneratePartitionKeyQueryDateTime()
    {
        var startDate = new DateTimeOffset(2024, 4, 1, 0, 0, 0, TimeSpan.FromHours(-5));
        var startTime = startDate.UtcDateTime;

        var endDate = new DateTimeOffset(2024, 4, 2, 0, 0, 0, TimeSpan.FromHours(-5));
        var endTime = endDate.UtcDateTime;

        var partitionKeyQuery = KeyGenerator.GeneratePartitionKeyQuery(startTime, endTime);
        partitionKeyQuery.Should().NotBeNull();
        partitionKeyQuery.Should().Be("(PartitionKey ge '2516902667999999999') and (PartitionKey lt '2516903531999999999')");
    }

    [Fact]
    public void GeneratePartitionKeyQueryDateTimeOffset()
    {
        var startTime = new DateTimeOffset(2024, 4, 1, 0, 0, 0, TimeSpan.FromHours(-5));
        var endTime = new DateTimeOffset(2024, 4, 2, 0, 0, 0, TimeSpan.FromHours(-5));

        var partitionKeyQuery = KeyGenerator.GeneratePartitionKeyQuery(startTime, endTime);
        partitionKeyQuery.Should().NotBeNull();
        partitionKeyQuery.Should().Be("(PartitionKey ge '2516902667999999999') and (PartitionKey lt '2516903531999999999')");
    }

}
