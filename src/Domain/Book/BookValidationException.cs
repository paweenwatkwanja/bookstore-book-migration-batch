namespace BookMigrationBatch.Domain.Book;

public sealed class BookValidationException : Exception
{
    public BookValidationException(IReadOnlyList<string> missingFields)
        : base($"Book validation failed. Missing required field(s): {string.Join(", ", missingFields)}")
    {
        this.MissingFields = missingFields;
    }

    public IReadOnlyList<string> MissingFields { get; }
}
