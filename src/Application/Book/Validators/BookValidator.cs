using BookMigrationBatch.Application.Book.Abstractions;
using BookMigrationBatch.Domain.Book;
using DomainBook = BookMigrationBatch.Domain.Book.Book;

namespace BookMigrationBatch.Application.Book.Validators;

public sealed class BookValidator : IBookValidator
{
    public void Validate(DomainBook book)
    {
        List<string> missingFields = new List<string>();

        if (string.IsNullOrWhiteSpace(book.Isbn))
        {
            missingFields.Add(nameof(DomainBook.Isbn));
        }

        if (string.IsNullOrWhiteSpace(book.Title))
        {
            missingFields.Add(nameof(DomainBook.Title));
        }

        if (string.IsNullOrWhiteSpace(book.Author))
        {
            missingFields.Add(nameof(DomainBook.Author));
        }

        if (missingFields.Count > 0)
        {
            throw new BookValidationException(missingFields);
        }
    }
}
