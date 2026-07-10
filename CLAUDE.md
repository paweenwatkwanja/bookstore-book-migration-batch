# Project Overview
- The objective of this project is to read a CSV file from AWS S3, validate the data, and insert them into a MongoDB database.

## Tech Stack
- C# .NET (Version 10)
- MongoDB (Version 8.0)
- AWS S3 (Production)
- Floci (Local and Development) *Do not use LocalStack
- Docker
- MSTest for Unit Testing
- Testcontainers for MongoDB integration tests

## Architecture
- Clean Code

## Project Structure
├── .dockerignore
├── .gitignore
├── BookMigrationBatch.sln
├── Dockerfile
├── README.md
├── docker-compose.yaml
├── init-mongo.sh
├── init-s3.sh                          # bootstraps the bucket in Floci on startup
├── Properties/
├── src/
│   ├── Domain/
│   │   └── Book/
│   │       ├── Book.cs                 # core entity: Isbn, Title, Author, etc.
│   │       └── BookValidationException.cs
│   │
│   ├── Application/
│   │   └── Book/
│   │       ├── Abstractions/           # ports the batch job depends on
│   │       │   ├── IBookFileReader.cs      # reads raw CSV from a source (S3)
│   │       │   ├── IBookRepository.cs      # persists valid books (Mongo)
│   │       │   └── IBookValidator.cs
│   │       ├── UseCases/
│   │       │   └── ProcessBookBatchUseCase.cs   # orchestrates: read -> parse -> validate -> insert -> report
│   │       ├── Validators/
│   │       │   └── BookValidator.cs    # enforces ISBN + Title + Author required
│   │       └── Models/
│   │           ├── BookCsvRecord.cs    # raw strongly-typed row shape straight off the CSV
│   │           └── BatchResult.cs      # summary: total rows, inserted, rejected, errors[]
│   │
│   ├── Infrastructure/
│   │   ├── Storage/
│   │   │   └── S3BookFileReader.cs     # implements IBookFileReader against Floci/S3
│   │   ├── Parsing/
│   │   │   └── CsvBookParser.cs        # CSV -> BookCsvRecord
│   │   └── Persistence/
│   │       ├── MongoDbContext.cs
│   │       └── MongoBookRepository.cs  # implements IBookRepository
│   │
│   ├── BatchJobs/
│   │   └── BookMigrateJob.cs            # entry point that runs ProcessBookBatchUseCase
│   │                                   # (hosted service / scheduled trigger / console entry)
│   │
│   ├── Program.cs
│   ├── appsettings.Development.json
│   └── appsettings.json
└── tests/'

## Coding Rules
- Use strong type variables only (no var or object keywords)
- One class per file, file name matches class name



