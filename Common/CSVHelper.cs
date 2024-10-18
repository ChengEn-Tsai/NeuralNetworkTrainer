namespace DotNetAssignment2.Common
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    public class CsvHelper
    {
        public static List<Dictionary<string, string>> ParseCsvToDictionary(string filePath)
        {
            var result = new List<Dictionary<string, string>>();

            try
            {
                using (var reader = new StreamReader(filePath))
                {
                    var headerLine = reader.ReadLine();
                    if (headerLine == null)
                    {
                        throw new Exception("CSV file is empty.");
                    }

                    var headers = headerLine.Split(',');

                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        if (line == null) continue;

                        var values = line.Split(',');

                        var row = new Dictionary<string, string>();
                        for (int i = 0; i < headers.Length; i++)
                        {
                            var value = i < values.Length ? values[i] : string.Empty;
                            row[headers[i]] = value;
                        }

                        result.Add(row);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while parsing CSV: {ex.Message}");
            }

            return result;
        }
    }

}
