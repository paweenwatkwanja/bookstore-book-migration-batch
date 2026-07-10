using DomainBook = BookMigrationBatch.Domain.Book.Book;

namespace BookMigrationBatch.Application.Book.Abstractions;

public interface IBookRepository
{
    Task InsertAsync(DomainBook book, CancellationToken cancellationToken);
}
