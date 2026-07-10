namespace BookMigrationBatch.BatchJobs;

public interface IBatchJob
{
    Task<int> ExecuteAsync(CancellationToken cancellationToken);
}
