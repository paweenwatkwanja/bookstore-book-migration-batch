using BookMigrationBatch.Application.Book.Validators;
using BookMigrationBatch.Domain.Book;
using DomainBook = BookMigrationBatch.Domain.Book.Book;

namespace BookMigrationBatch.UnitTests;

[TestClass]
public sealed class BookValidatorTests
{
    private readonly BookValidator validator = new BookValidator();

    [TestMethod]
    public void Validate_WithAllRequiredFields_DoesNotThrow()
    {
        DomainBook book = CreateValidBook();

        this.validator.Validate(book);
    }

    [TestMethod]
    public void Validate_WithMissingOptionalFields_DoesNotThrow()
    {
        DomainBook book = new DomainBook
        {
            Isbn = "0000000000",
            Title = "Some Title",
            Author = "Some Author",
            Publisher = null,
            PublicationYear = null,
            ImageUrl = null,
        };

        this.validator.Validate(book);
    }

    [TestMethod]
    public void Validate_WithMissingIsbn_ThrowsBookValidationException()
    {
        DomainBook book = CreateValidBook(isbn: string.Empty);

        BookValidationException exception = Assert.ThrowsExactly<BookValidationException>(() => this.validator.Validate(book));

        CollectionAssert.Contains(exception.MissingFields.ToList(), nameof(DomainBook.Isbn));
    }

    [TestMethod]
    public void Validate_WithMissingTitle_ThrowsBookValidationException()
    {
        DomainBook book = CreateValidBook(title: string.Empty);

        BookValidationException exception = Assert.ThrowsExactly<BookValidationException>(() => this.validator.Validate(book));

        CollectionAssert.Contains(exception.MissingFields.ToList(), nameof(DomainBook.Title));
    }

    [TestMethod]
    public void Validate_WithMissingAuthor_ThrowsBookValidationException()
    {
        DomainBook book = CreateValidBook(author: string.Empty);

        BookValidationException exception = Assert.ThrowsExactly<BookValidationException>(() => this.validator.Validate(book));

        CollectionAssert.Contains(exception.MissingFields.ToList(), nameof(DomainBook.Author));
    }

    [TestMethod]
    public void Validate_WithWhitespaceOnlyTitle_ThrowsBookValidationException()
    {
        DomainBook book = CreateValidBook(title: "   ");

        Assert.ThrowsExactly<BookValidationException>(() => this.validator.Validate(book));
    }

    private static DomainBook CreateValidBook(string? isbn = null, string? title = null, string? author = null)
    {
        return new DomainBook
        {
            Isbn = isbn ?? "0316769487",
            Title = title ?? "The Catcher in the Rye",
            Author = author ?? "J.D. Salinger",
            Publisher = "Little, Brown and Company",
            PublicationYear = 1951,
            ImageUrl = "http://images.example.com/0316769487.jpg",
        };
    }
}
