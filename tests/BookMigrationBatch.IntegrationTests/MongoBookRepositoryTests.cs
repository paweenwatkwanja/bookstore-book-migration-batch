using BookMigrationBatch.Infrastructure.Configuration;
using BookMigrationBatch.Infrastructure.Persistence;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Testcontainers.MongoDb;
using DomainBook = BookMigrationBatch.Domain.Book.Book;

namespace BookMigrationBatch.IntegrationTests;

[TestClass]
public sealed class MongoBookRepositoryTests
{
    private static MongoDbContainer mongoContainer = null!;
    private static MongoDbContext dbContext = null!;
    private static MongoBookRepository repository = null!;

    [ClassInitialize]
    public static async Task ClassInitializeAsync(TestContext context)
    {
        mongoContainer = new MongoDbBuilder("mongo:8.0").Build();
        await mongoContainer.StartAsync();

        MongoDbOptions options = new MongoDbOptions
        {
            ConnectionString = mongoContainer.GetConnectionString(),
            DatabaseName = "bookstore",
            CollectionName = "books",
        };

        dbContext = new MongoDbContext(Options.Create(options));

        await dbContext.Books.Indexes.CreateOneAsync(
            new CreateIndexModel<DomainBook>(
                Builders<DomainBook>.IndexKeys.Ascending(book => book.Isbn),
                new CreateIndexOptions { Unique = true }));

        repository = new MongoBookRepository(dbContext, NullLogger<MongoBookRepository>.Instance);
    }

    [ClassCleanup]
    public static async Task ClassCleanupAsync()
    {
        await mongoContainer.DisposeAsync();
    }

    [TestMethod]
    public async Task InsertAsync_WithValidBook_PersistsToCollection()
    {
        DomainBook book = new DomainBook
        {
            Isbn = "9780000000001",
            Title = "Integration Test Book",
            Author = "Test Author",
            Publisher = "Test Publisher",
            PublicationYear = 2020,
            ImageUrl = "https://images.example.com/covers/L/9780000000001.jpg",
        };

        await repository.InsertAsync(book, CancellationToken.None);

        DomainBook? persisted = await dbContext.Books.Find(existing => existing.Isbn == book.Isbn).FirstOrDefaultAsync();

        Assert.IsNotNull(persisted);
        Assert.AreEqual(book.Title, persisted.Title);
        Assert.AreEqual(book.Author, persisted.Author);
        Assert.AreEqual(book.PublicationYear, persisted.PublicationYear);
    }

    [TestMethod]
    public async Task InsertAsync_WithDuplicateIsbn_ThrowsMongoWriteException()
    {
        DomainBook book = new DomainBook
        {
            Isbn = "9780000000002",
            Title = "Duplicate Test Book",
            Author = "Test Author",
        };

        await repository.InsertAsync(book, CancellationToken.None);

        await Assert.ThrowsExactlyAsync<MongoWriteException>(() => repository.InsertAsync(book, CancellationToken.None));
    }
}
