using BookMigrationBatch.Application.Book.Models;

namespace BookMigrationBatch.Application.Book.Abstractions;

public interface IBookFileReader
{
    IAsyncEnumerable<BookCsvRecord> ReadBooksAsync(string bucketName, string objectKey, CancellationToken cancellationToken);
}
