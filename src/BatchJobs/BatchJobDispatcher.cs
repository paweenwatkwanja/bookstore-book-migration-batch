using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BookMigrationBatch.BatchJobs;

public sealed class BatchJobDispatcher : IBatchJobDispatcher
{
    private readonly IServiceProvider serviceProvider;
    private readonly ILogger<BatchJobDispatcher> logger;

    public BatchJobDispatcher(IServiceProvider serviceProvider, ILogger<BatchJobDispatcher> logger)
    {
        this.serviceProvider = serviceProvider;
        this.logger = logger;
    }

    public async Task<int> DispatchAsync(string jobName, CancellationToken cancellationToken)
    {
        IBatchJob? job = this.serviceProvider.GetKeyedService<IBatchJob>(jobName);

        if (job is null)
        {
            this.logger.LogError("Unknown job name: {JobName}", jobName);
            return 1;
        }

        return await job.ExecuteAsync(cancellationToken);
    }
}
