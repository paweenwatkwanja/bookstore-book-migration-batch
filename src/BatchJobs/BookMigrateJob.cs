using BookMigrationBatch.Application.Book.Models;
using BookMigrationBatch.Application.Book.UseCases;
using BookMigrationBatch.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BookMigrationBatch.BatchJobs;

public sealed class BookMigrateJob : IBatchJob
{
    public const string JobName = "BookMigrate";

    private readonly ProcessBookBatchUseCase useCase;
    private readonly S3Options s3Options;
    private readonly ILogger<BookMigrateJob> logger;

    public BookMigrateJob(ProcessBookBatchUseCase useCase, IOptions<S3Options> s3Options, ILogger<BookMigrateJob> logger)
    {
        this.useCase = useCase;
        this.s3Options = s3Options.Value;
        this.logger = logger;
    }

    public async Task<int> ExecuteAsync(CancellationToken cancellationToken)
    {
        BatchResult result = await this.useCase.ExecuteAsync(this.s3Options.BucketName, this.s3Options.ObjectKey, cancellationToken);

        this.logger.LogInformation(
            "Book migration complete. TotalRows={TotalRows} Inserted={InsertedCount} Rejected={RejectedCount}",
            result.TotalRows,
            result.InsertedCount,
            result.RejectedCount);

        foreach (string error in result.Errors)
        {
            this.logger.LogWarning("{Error}", error);
        }

        return 0;
    }
}
