
using CsvHelper;
using System.Globalization;
namespace WebApiPlayground;


static class ProductsCsv
{
    internal static string GenerateCsv(this IEnumerable<Product> records) {
        using(var memory = new MemoryStream())
        using(var writer = new StreamWriter(memory))
        using(var reader = new StreamReader(memory))
        using(var csv = new CsvWriter(writer, CultureInfo.InvariantCulture)) {
            csv.WriteRecords(records);
            writer.Flush();
            memory.Position = 0;
            return reader.ReadToEnd();
        }
    }

    internal static async Task Process(Stream source, Func<Product, Task> toDo) {
        using(var reader = new StreamReader(source))
        using(var csv = new CsvReader(reader, CultureInfo.InvariantCulture)) {
            var content = csv.GetRecordsAsync<Product>();
            await foreach(var newOrUpdated in content) {
                await toDo(newOrUpdated);
            };
        }
    }
    
}
