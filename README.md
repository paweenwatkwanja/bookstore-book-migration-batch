# Book Migration Batch

Reads a book CSV file from AWS S3, validates the records, and inserts them into MongoDB. Built as a batch job intended to run under AWS Batch, with a local development stack based on Docker.

## Tech stack

- C# / .NET 10
- MongoDB 8.0
- AWS S3 (production) / [Floci](https://floci.io) (local & development — an S3-compatible emulator)
- MSTest + Testcontainers for integration tests
- Docker / Docker Compose

## Architecture

Clean Architecture, organized as folders/namespaces within a single project (`src/BookMigrationBatch.csproj`) rather than separate class libraries, to keep the project's size manageable:

- `Domain/Book` — the `Book` entity and validation exception.
- `Application/Book` — CSV row model, batch result model, abstractions (`IBookFileReader`, `IBookRepository`, `IBookValidator`), the validator, and the `ProcessBookBatchUseCase` orchestrator (read → parse → map → validate → insert → report).
- `Infrastructure` — S3 (`S3BookFileReader`), CSV parsing (`CsvBookParser`), MongoDB (`MongoDbContext`, `MongoBookRepository`), and strongly-typed configuration options.
- `BatchJobs` — an extensible job dispatcher (`IBatchJobDispatcher`) backed by keyed dependency injection, and `BookMigrateJob`, the first registered job.
- `Program.cs` — composition root: reads the job name from the command line, wires up DI, optionally overlays the MongoDB connection string from AWS Secrets Manager in Production, and runs the dispatcher.

## Running the job

The job name is passed as the first command-line argument (this mirrors how an AWS Batch job definition passes the job name):

```bash
dotnet run --project src/BookMigrationBatch.csproj -- BookMigrate
```

Adding a new job type later only requires implementing `IBatchJob` and registering it with `AddKeyedSingleton<IBatchJob, TJob>("JobName")` in `Program.cs` — `BatchJobDispatcher` itself never changes.

## Local development stack

```bash
docker compose up --build
```

This starts:
- `mongo` — MongoDB 8.0, initialized via `init-mongo.sh` (creates the `bookstore` database, the `books` collection, and a unique index on `Isbn`).
- `floci` — the local S3-compatible emulator, listening on port 4566.
- `floci-init` — a one-shot container that waits for Floci to be ready, creates the `bookstore-books` bucket, and uploads `tests/TestData/BX-Books-Sample.csv` (50 sample records) as `books/BX-Books.csv`.
- `batch-job` — builds the app image and runs the `BookMigrate` job against Floci and Mongo.

Verify the result:

```bash
docker compose exec mongo mongosh bookstore --eval "db.books.countDocuments()"
```

## Configuration

| Setting | `appsettings.json` (Production default) | `appsettings.Development.json` |
|---|---|---|
| `S3:ServiceUrl` | `null` (real AWS S3) | `http://floci:4566` |
| `S3:ForcePathStyle` | `false` | `true` |
| `MongoDb:ConnectionString` | empty — filled from Secrets Manager | `mongodb://mongo:27017` |
| `SecretsManager:Enabled` | `true` | `false` |

In Production, S3 access relies on the AWS Batch task's IAM role (default AWS credential chain) rather than static keys. `SecretsManager:SecretName` points at a secret containing the MongoDB connection string, fetched at startup and merged into configuration.

The environment is selected via the `DOTNET_ENVIRONMENT` variable; if unset, the Generic Host defaults to `Production`.

## Testing

```bash
dotnet test tests/BookMigrationBatch.UnitTests        # fast, no Docker required
dotnet test tests/BookMigrationBatch.IntegrationTests  # requires a running Docker daemon (Testcontainers)
```

## Notes

- `Properties/launchSettings.json` is checked in at the repo root per this project's layout convention. If your IDE doesn't pick up the debug profile automatically, note that `launchSettings.json` is normally expected at `src/Properties/launchSettings.json` (next to the `.csproj`) — this is a local-debugging convenience only and has no effect on the containerized/production path.
