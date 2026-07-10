namespace BookMigrationBatch.Infrastructure.Configuration;

public sealed class SecretsManagerOptions
{
    public bool Enabled { get; set; }

    public string SecretName { get; set; } = string.Empty;

    public string Region { get; set; } = "us-east-1";
}
