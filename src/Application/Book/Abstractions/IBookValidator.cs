using DomainBook = BookMigrationBatch.Domain.Book.Book;

namespace BookMigrationBatch.Application.Book.Abstractions;

public interface IBookValidator
{
    void Validate(DomainBook book);
}
