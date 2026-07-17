namespace BookMigrationBatch.Application.Book.Models;

public sealed class BatchResult
{
    public required int TotalRows { get; init; }

    public required int InsertedCount { get; init; }

    public required int RejectedCount { get; init; }

    public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();
}
