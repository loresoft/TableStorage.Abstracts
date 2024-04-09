using Azure.Data.Tables;

using Microsoft.Extensions.Logging;

using TableStorage.Abstracts.Tests.Models;

namespace TableStorage.Abstracts.Tests.Services;


public class UserRepository : TableRepository<User>
{
    public UserRepository(ILoggerFactory logFactory, TableServiceClient tableServiceClient)
        : base(logFactory, tableServiceClient)
    {
    }

    protected override void BeforeSave(User entity)
    {
        // use email as partition key
        entity.PartitionKey = entity.Email;

        base.BeforeSave(entity);
    }

    protected override string GetTableName() => "UserMembership";
}
