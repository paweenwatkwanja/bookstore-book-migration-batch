namespace BookMigrationBatch.Infrastructure.Configuration;

public sealed class S3Options
{
    public string BucketName { get; set; } = string.Empty;

    public string ObjectKey { get; set; } = string.Empty;

    public string Region { get; set; } = "us-east-1";

    public string? ServiceUrl { get; set; }

    public bool ForcePathStyle { get; set; }
}
