using System.Globalization;
using BookMigrationBatch.Application.Book.Models;
using CsvHelper;
using CsvHelper.Configuration;

namespace BookMigrationBatch.Infrastructure.Parsing;

public sealed class CsvBookParser
{
    public IEnumerable<BookCsvRecord> Parse(Stream csvStream)
    {
        CsvConfiguration configuration = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = ";",
            HasHeaderRecord = true,
            TrimOptions = TrimOptions.Trim,
        };

        using StreamReader streamReader = new StreamReader(csvStream);
        using CsvReader csvReader = new CsvReader(streamReader, configuration);

        return csvReader.GetRecords<BookCsvRecord>().ToList();
    }
}
