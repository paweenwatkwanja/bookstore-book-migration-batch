using BookMigrationBatch.Application.Book.Abstractions;
using BookMigrationBatch.Application.Book.Models;
using BookMigrationBatch.Domain.Book;
using Microsoft.Extensions.Logging;
using DomainBook = BookMigrationBatch.Domain.Book.Book;

namespace BookMigrationBatch.Application.Book.UseCases;

public sealed class ProcessBookBatchUseCase
{
    private readonly IBookFileReader bookFileReader;
    private readonly IBookValidator bookValidator;
    private readonly IBookRepository bookRepository;
    private readonly ILogger<ProcessBookBatchUseCase> logger;

    public ProcessBookBatchUseCase(
        IBookFileReader bookFileReader,
        IBookValidator bookValidator,
        IBookRepository bookRepository,
        ILogger<ProcessBookBatchUseCase> logger)
    {
        this.bookFileReader = bookFileReader;
        this.bookValidator = bookValidator;
        this.bookRepository = bookRepository;
        this.logger = logger;
    }

    public async Task<BatchResult> ExecuteAsync(string bucketName, string objectKey, CancellationToken cancellationToken)
    {
        int totalRows = 0;
        int insertedCount = 0;
        List<string> errors = new List<string>();

        await foreach (BookCsvRecord record in this.bookFileReader.ReadBooksAsync(bucketName, objectKey, cancellationToken))
        {
            totalRows++;
            DomainBook book = MapToBook(record);

            try
            {
                this.bookValidator.Validate(book);
                await this.bookRepository.InsertAsync(book, cancellationToken);
                insertedCount++;
            }
            catch (BookValidationException ex)
            {
                this.logger.LogWarning("Row {RowNumber} rejected: {Message}", totalRows, ex.Message);
                errors.Add($"Row {totalRows} (ISBN '{record.Isbn}'): {ex.Message}");
            }
            catch (Exception ex)
            {
                this.logger.LogWarning(ex, "Row {RowNumber} rejected during insert", totalRows);
                errors.Add($"Row {totalRows} (ISBN '{record.Isbn}'): {ex.Message}");
            }
        }

        return new BatchResult
        {
            TotalRows = totalRows,
            InsertedCount = insertedCount,
            RejectedCount = totalRows - insertedCount,
            Errors = errors,
        };
    }

    private static DomainBook MapToBook(BookCsvRecord record)
    {
        int? publicationYear = int.TryParse(record.Year, out int parsedYear) ? parsedYear : null;

        return new DomainBook
        {
            Isbn = record.Isbn,
            Title = record.Title,
            Author = record.Author,
            Publisher = record.Publisher,
            PublicationYear = publicationYear,
            ImageUrl = record.ImageUrlLarge,
        };
    }
}
