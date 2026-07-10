using BookMigrationBatch.Application.Book.Abstractions;
using BookMigrationBatch.Application.Book.Models;
using BookMigrationBatch.Application.Book.UseCases;
using BookMigrationBatch.Application.Book.Validators;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using DomainBook = BookMigrationBatch.Domain.Book.Book;

namespace BookMigrationBatch.UnitTests;

[TestClass]
public sealed class ProcessBookBatchUseCaseTests
{
    [TestMethod]
    public async Task ExecuteAsync_WithMixedValidAndInvalidRows_ReturnsCorrectCounts()
    {
        List<BookCsvRecord> records = new List<BookCsvRecord>
        {
            new BookCsvRecord { Isbn = "111", Title = "Valid Book One", Author = "Author One", Year = "2001" },
            new BookCsvRecord { Isbn = string.Empty, Title = "Missing ISBN", Author = "Author Two", Year = "2002" },
            new BookCsvRecord { Isbn = "333", Title = "Valid Book Two", Author = "Author Three", Year = "2003" },
        };

        Mock<IBookFileReader> fileReaderMock = new Mock<IBookFileReader>();
        fileReaderMock
            .Setup(reader => reader.ReadBooksAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(ToAsyncEnumerable(records));

        Mock<IBookRepository> repositoryMock = new Mock<IBookRepository>();
        repositoryMock
            .Setup(repository => repository.InsertAsync(It.IsAny<DomainBook>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        ProcessBookBatchUseCase useCase = new ProcessBookBatchUseCase(
            fileReaderMock.Object,
            new BookValidator(),
            repositoryMock.Object,
            NullLogger<ProcessBookBatchUseCase>.Instance);

        BatchResult result = await useCase.ExecuteAsync("bucket", "key", CancellationToken.None);

        Assert.AreEqual(3, result.TotalRows);
        Assert.AreEqual(2, result.InsertedCount);
        Assert.AreEqual(1, result.RejectedCount);
        Assert.HasCount(1, result.Errors);
        repositoryMock.Verify(repository => repository.InsertAsync(It.IsAny<DomainBook>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [TestMethod]
    public async Task ExecuteAsync_WhenRepositoryThrowsForOneRow_ContinuesProcessingRemainingRows()
    {
        List<BookCsvRecord> records = new List<BookCsvRecord>
        {
            new BookCsvRecord { Isbn = "111", Title = "Book One", Author = "Author One", Year = "2001" },
            new BookCsvRecord { Isbn = "222", Title = "Book Two", Author = "Author Two", Year = "2002" },
        };

        Mock<IBookFileReader> fileReaderMock = new Mock<IBookFileReader>();
        fileReaderMock
            .Setup(reader => reader.ReadBooksAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(ToAsyncEnumerable(records));

        Mock<IBookRepository> repositoryMock = new Mock<IBookRepository>();
        repositoryMock
            .Setup(repository => repository.InsertAsync(It.Is<DomainBook>(book => book.Isbn == "111"), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("duplicate key"));
        repositoryMock
            .Setup(repository => repository.InsertAsync(It.Is<DomainBook>(book => book.Isbn == "222"), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        ProcessBookBatchUseCase useCase = new ProcessBookBatchUseCase(
            fileReaderMock.Object,
            new BookValidator(),
            repositoryMock.Object,
            NullLogger<ProcessBookBatchUseCase>.Instance);

        BatchResult result = await useCase.ExecuteAsync("bucket", "key", CancellationToken.None);

        Assert.AreEqual(2, result.TotalRows);
        Assert.AreEqual(1, result.InsertedCount);
        Assert.AreEqual(1, result.RejectedCount);
        repositoryMock.Verify(repository => repository.InsertAsync(It.IsAny<DomainBook>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    private static async IAsyncEnumerable<BookCsvRecord> ToAsyncEnumerable(IEnumerable<BookCsvRecord> records)
    {
        foreach (BookCsvRecord record in records)
        {
            yield return record;
        }

        await Task.CompletedTask;
    }
}
