using System.Text.Json;
using Amazon;
using Amazon.S3;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using BookMigrationBatch.Application.Book.Abstractions;
using BookMigrationBatch.Application.Book.UseCases;
using BookMigrationBatch.Application.Book.Validators;
using BookMigrationBatch.BatchJobs;
using BookMigrationBatch.Infrastructure.Configuration;
using BookMigrationBatch.Infrastructure.Parsing;
using BookMigrationBatch.Infrastructure.Persistence;
using BookMigrationBatch.Infrastructure.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BookMigrationBatch;

public sealed class Program
{
    public static async Task<int> Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.Error.WriteLine("Usage: BookMigrationBatch <job-name> [additional host args...]");
            return 1;
        }

        string jobName = args[0];
        string[] hostArgs = args.Length > 1 ? args[1..] : Array.Empty<string>();

        string environmentName = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production";
        IConfigurationRoot bootstrapConfiguration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile($"appsettings.{environmentName}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        SecretsManagerOptions secretsManagerOptions = new SecretsManagerOptions();
        bootstrapConfiguration.GetSection("SecretsManager").Bind(secretsManagerOptions);

        Dictionary<string, string?>? secretOverrides = null;

        if (secretsManagerOptions.Enabled)
        {
            secretOverrides = await FetchMongoDbSecretOverridesAsync(secretsManagerOptions);
        }

        HostApplicationBuilder builder = Host.CreateApplicationBuilder(hostArgs);

        if (secretOverrides is not null)
        {
            builder.Configuration.AddInMemoryCollection(secretOverrides);
        }

        builder.Services.Configure<S3Options>(builder.Configuration.GetSection("S3"));
        builder.Services.Configure<MongoDbOptions>(builder.Configuration.GetSection("MongoDb"));
        builder.Services.Configure<SecretsManagerOptions>(builder.Configuration.GetSection("SecretsManager"));

        builder.Services.AddSingleton<IAmazonS3>(_ => CreateS3Client(builder.Configuration));

        builder.Services.AddSingleton<MongoDbContext>();
        builder.Services.AddSingleton<IBookRepository, MongoBookRepository>();
        builder.Services.AddSingleton<CsvBookParser>();
        builder.Services.AddSingleton<IBookFileReader, S3BookFileReader>();
        builder.Services.AddSingleton<IBookValidator, BookValidator>();
        builder.Services.AddSingleton<ProcessBookBatchUseCase>();
        builder.Services.AddKeyedSingleton<IBatchJob, BookMigrateJob>(BookMigrateJob.JobName);
        builder.Services.AddSingleton<IBatchJobDispatcher, BatchJobDispatcher>();

        using IHost host = builder.Build();

        try
        {
            IBatchJobDispatcher dispatcher = host.Services.GetRequiredService<IBatchJobDispatcher>();
            return await dispatcher.DispatchAsync(jobName, CancellationToken.None);
        }
        catch (Exception ex)
        {
            ILogger<Program> logger = host.Services.GetRequiredService<ILogger<Program>>();
            logger.LogCritical(ex, "Fatal error while running job {JobName}", jobName);
            return 1;
        }
    }

    private static async Task<Dictionary<string, string?>> FetchMongoDbSecretOverridesAsync(SecretsManagerOptions secretsManagerOptions)
    {
        using AmazonSecretsManagerClient secretsManagerClient = new AmazonSecretsManagerClient(RegionEndpoint.GetBySystemName(secretsManagerOptions.Region));

        GetSecretValueRequest request = new GetSecretValueRequest
        {
            SecretId = secretsManagerOptions.SecretName,
        };

        GetSecretValueResponse response = await secretsManagerClient.GetSecretValueAsync(request);
        MongoDbSecret? mongoDbSecret = JsonSerializer.Deserialize<MongoDbSecret>(response.SecretString);

        return new Dictionary<string, string?>
        {
            ["MongoDb:ConnectionString"] = mongoDbSecret?.ConnectionString,
        };
    }

    private static AmazonS3Client CreateS3Client(IConfiguration configuration)
    {
        S3Options s3Options = new S3Options();
        configuration.GetSection("S3").Bind(s3Options);

        AmazonS3Config s3Config = new AmazonS3Config
        {
            RegionEndpoint = RegionEndpoint.GetBySystemName(s3Options.Region),
        };

        if (!string.IsNullOrWhiteSpace(s3Options.ServiceUrl))
        {
            s3Config.ServiceURL = s3Options.ServiceUrl;
            s3Config.ForcePathStyle = s3Options.ForcePathStyle;
        }

        return new AmazonS3Client(s3Config);
    }
}
