namespace BookMigrationBatch.BatchJobs;

public interface IBatchJobDispatcher
{
    Task<int> DispatchAsync(string jobName, CancellationToken cancellationToken);
}
