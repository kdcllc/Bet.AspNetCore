using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using CsvHelper;
using CsvHelper.Configuration;

namespace Bet.Extensions.ML.Data
{
    public sealed class LoadFromEmbededResource
    {
        public static List<TData> GetRecords<TData>(
            string fileName,
            Configuration configuration) where TData : class
        {
            var assembly = typeof(TData).GetTypeInfo().Assembly;

            using (var resource = assembly.GetManifestResourceStream($"{assembly.GetName().Name}.{fileName}"))
            using (var reader = new StreamReader(resource))
            using (var csv = new CsvReader(reader, configuration))
            {
                var records = csv.GetRecords<TData>();
                return records.ToList();
            }
        }

        public static List<TData> GetRecords<TData>(
            string fileName,
            string delimiter = ",",
            bool hasHeaderRecord = true) where TData : class
        {
            var configReader = new Configuration
            {
                HasHeaderRecord = hasHeaderRecord,
                Delimiter = delimiter,
                BadDataFound = null,
                HeaderValidated = null,
                MissingFieldFound = null,
            };

            return GetRecords<TData>(fileName, configuration: configReader);
        }
    }
}
