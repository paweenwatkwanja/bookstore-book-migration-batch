namespace BookMigrationBatch.Infrastructure.Configuration;

public sealed class MongoDbSecret
{
    public string ConnectionString { get; set; } = string.Empty;
}
