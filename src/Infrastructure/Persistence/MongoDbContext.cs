using BookMigrationBatch.Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using DomainBook = BookMigrationBatch.Domain.Book.Book;

namespace BookMigrationBatch.Infrastructure.Persistence;

public sealed class MongoDbContext
{
    public MongoDbContext(IOptions<MongoDbOptions> options)
    {
        MongoDbOptions mongoDbOptions = options.Value;
        MongoClient client = new MongoClient(mongoDbOptions.ConnectionString);
        IMongoDatabase database = client.GetDatabase(mongoDbOptions.DatabaseName);
        this.Books = database.GetCollection<DomainBook>(mongoDbOptions.CollectionName);
    }

    public IMongoCollection<DomainBook> Books { get; }
}
