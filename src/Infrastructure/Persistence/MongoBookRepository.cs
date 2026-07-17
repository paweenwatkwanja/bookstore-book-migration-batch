using BookMigrationBatch.Application.Book.Abstractions;
using Microsoft.Extensions.Logging;
using DomainBook = BookMigrationBatch.Domain.Book.Book;

namespace BookMigrationBatch.Infrastructure.Persistence;

public sealed class MongoBookRepository : IBookRepository
{
    private readonly MongoDbContext dbContext;
    private readonly ILogger<MongoBookRepository> logger;

    public MongoBookRepository(MongoDbContext dbContext, ILogger<MongoBookRepository> logger)
    {
        this.dbContext = dbContext;
        this.logger = logger;
    }

    public async Task InsertAsync(DomainBook book, CancellationToken cancellationToken)
    {
        await this.dbContext.Books.InsertOneAsync(book, options: null, cancellationToken);
        this.logger.LogDebug("Inserted book with ISBN {Isbn}", book.Isbn);
    }
}
