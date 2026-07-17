using MongoDB.Bson.Serialization.Attributes;

namespace BookMigrationBatch.Domain.Book;

[BsonIgnoreExtraElements]
public sealed class Book
{
    public required string Isbn { get; init; }

    public required string Title { get; init; }

    public required string Author { get; init; }

    public string? Publisher { get; init; }

    public int? PublicationYear { get; init; }

    public string? ImageUrl { get; init; }
}
