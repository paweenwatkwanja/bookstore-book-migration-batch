using CsvHelper.Configuration.Attributes;

namespace BookMigrationBatch.Application.Book.Models;

public sealed class BookCsvRecord
{
    [Name("ISBN")]
    public string Isbn { get; set; } = string.Empty;

    [Name("Book-Title")]
    public string Title { get; set; } = string.Empty;

    [Name("Book-Author")]
    public string Author { get; set; } = string.Empty;

    [Name("Year-Of-Publication")]
    public string? Year { get; set; }

    [Name("Publisher")]
    public string? Publisher { get; set; }

    [Name("Image-URL-S")]
    public string? ImageUrlSmall { get; set; }

    [Name("Image-URL-M")]
    public string? ImageUrlMedium { get; set; }

    [Name("Image-URL-L")]
    public string? ImageUrlLarge { get; set; }
}
