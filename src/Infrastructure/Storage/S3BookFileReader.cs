using System.Runtime.CompilerServices;
using Amazon.S3;
using Amazon.S3.Model;
using BookMigrationBatch.Application.Book.Abstractions;
using BookMigrationBatch.Application.Book.Models;
using BookMigrationBatch.Infrastructure.Parsing;
using Microsoft.Extensions.Logging;

namespace BookMigrationBatch.Infrastructure.Storage;

public sealed class S3BookFileReader : IBookFileReader
{
    private readonly IAmazonS3 s3Client;
    private readonly CsvBookParser csvParser;
    private readonly ILogger<S3BookFileReader> logger;

    public S3BookFileReader(IAmazonS3 s3Client, CsvBookParser csvParser, ILogger<S3BookFileReader> logger)
    {
        this.s3Client = s3Client;
        this.csvParser = csvParser;
        this.logger = logger;
    }

    public async IAsyncEnumerable<BookCsvRecord> ReadBooksAsync(
        string bucketName,
        string objectKey,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        this.logger.LogInformation("Downloading s3://{Bucket}/{Key}", bucketName, objectKey);

        GetObjectRequest request = new GetObjectRequest
        {
            BucketName = bucketName,
            Key = objectKey,
        };

        using GetObjectResponse response = await this.s3Client.GetObjectAsync(request, cancellationToken);
        IEnumerable<BookCsvRecord> records = this.csvParser.Parse(response.ResponseStream);

        foreach (BookCsvRecord record in records)
        {
            yield return record;
        }
    }
}
