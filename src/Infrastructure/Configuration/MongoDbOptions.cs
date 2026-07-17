namespace BookMigrationBatch.Infrastructure.Configuration;

public sealed class MongoDbOptions
{
    public string ConnectionString { get; set; } = string.Empty;

    public string DatabaseName { get; set; } = "bookstore";

    public string CollectionName { get; set; } = "books";
}
