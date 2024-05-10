using Microsoft.Extensions.DependencyInjection;

using TableStorage.Abstracts.Tests.Services;

namespace TableStorage.Abstracts.Tests;

public class LogEventRepositoryTest : DatabaseTestBase
{
    public LogEventRepositoryTest(ITestOutputHelper output, DatabaseFixture databaseFixture)
        : base(output, databaseFixture)
    {
    }

    [Fact]
    public async Task QueryTest()
    {
        var repository = Services.GetRequiredService<ILogEventRepository>();
        repository.Should().NotBeNull();

        var today = DateOnly.FromDateTime(DateTime.Now);

        var result = await repository.Query(today);
        result.Should().NotBeNull();
    }
}
