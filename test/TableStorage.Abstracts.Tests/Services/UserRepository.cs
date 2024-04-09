using Azure.Data.Tables;

using Microsoft.Extensions.Logging;

using TableStorage.Abstracts.Tests.Models;

namespace TableStorage.Abstracts.Tests.Services;

public class UserRepository : TableRepository<User>, IUserRepository
{
    public UserRepository(ILoggerFactory logFactory, TableServiceClient tableServiceClient)
        : base(logFactory, tableServiceClient)
    {
    }
}
