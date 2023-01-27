
using System.Text;
using System.Text.RegularExpressions;

static class ProductsCsv
{
    const int nrOfColumns = 5;

    //there is this nugget: https://github.com/jitbit/CsvExport but using it won't save much code and it's much less performant...
    //this I can easily optimize to work in streaming way - to write each line into file or send it over tcp so not using lot of memory
    internal static string Serialize(IEnumerable<Product> content) {
        var writer =new StringBuilder();
        foreach(var item in content)
            AppendItem(item);
        return writer.ToString();

        void AppendItem(Product item) =>
            writer.Append(
                WrapValue(item.Name)
                + WrapValue(item.Description)
                + WrapValue(item.WeightInKg)
                + WrapValue($"{item.WidthInCm}x{item.HeightInCm}x{item.DepthInCm}")
                + WrapValue((double)item.PricePer100 / 100.0, isLast: true));

        string WrapValue(object? value, bool isLast = false) =>
            $"\"{value ?? string.Empty}\"" + (isLast ? "\r\n" : ", ");
    }

    internal static bool Parse(string csv, out List<Product> result) {
        result = new List<Product>();
        var rows = csv.Split("\n\r".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        foreach(var line in rows) {
            if(ParseItem(line, out var products)) {
                result.AddRange(products);
            }
        }
        return result.Count() > 0;

        bool ParseItem(string dataLine, out List<Product> result) {
            result = new();
            var csvValues = SplitValues(dataLine);
            //if(values.Length % nrOfColumns != 0) {
            //    return false;
            //}
            for(int i = 0; i < csvValues.Length; i += nrOfColumns) { //because problems with line breaks
                var productValues = csvValues.Skip(i).Take(nrOfColumns).ToArray();
                var parsed = Parse(productValues);
                if(parsed is null) {
                    continue;
                }
                result.Add(parsed);
            }
            return result.Count > 0;
        }

        string[] SplitValues(string rowData) {
            var magic = new Regex(@"\s*""([^""]*)""|\G,\s*?([^, $""]+)", RegexOptions.Compiled); //it misses empty fields like ", ,"
            return magic.Matches(rowData).Select(CleanUpMatch).ToArray();
        }

        string CleanUpMatch(Match patternMatch) =>
            (string.IsNullOrEmpty(patternMatch.Groups[1].Value)
            ? patternMatch.Groups[2].Value
            : patternMatch.Groups[1].Value).Trim().Replace("\"", "");


        Product Parse(string[] values) {
            try {
                var dimensionsMagic = new Regex(@"");
                var dimensions = values[3].Split("x-".ToCharArray()); 
                if(dimensions.Length != 3) {
                    dimensions = null;
                }
                var another = new Product() {
                    Name = ParseValue<string>(values[0]),
                    Description = ParseValue<string>(values[1], string.Empty),
                    WeightInKg = ParseValue<float>(values[2], 0f),
                    WidthInCm = ParseValue<int>(dimensions?[0], 0),
                    HeightInCm = ParseValue<int>(dimensions?[1], 0),
                    DepthInCm = ParseValue<int>(dimensions?[2], 0),
                    PricePer100 = (int)(ParseValue<double>(values[4]) * 100)
                };
                return another;
                
            } catch(Exception error) {
                //some log or error outed in response would be appropriate
                return null;
            }
        }

        T ParseValue<T>(string encoded, object defaultValue = null) {
            encoded = encoded?.Trim().Replace("\"", "");
            if(IsMaaningNull()) {
                return (T)defaultValue;
            }
            object x = Type.GetTypeCode(typeof(T)) switch {
                TypeCode.Int32
                or TypeCode.Int64 => int.Parse(encoded),
                TypeCode.String => encoded,
                TypeCode.Double => double.Parse(encoded),
                TypeCode.Single => float.Parse(encoded),
                _ => throw new NotSupportedException()
            };
            return (T)x;

            bool IsMaaningNull() =>
                string.IsNullOrWhiteSpace(encoded) 
                || encoded.ToLower() == "null";
        }
    }
}
