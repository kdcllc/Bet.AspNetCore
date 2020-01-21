using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using CsvHelper;
using CsvHelper.Configuration;

namespace Bet.Extensions.ML.Helpers
{
    public static class FileHelper
    {
        public static List<TData> GetRecords<TData>(
            string fileLocation,
            Configuration configuration) where TData : class
        {
            using var reader = new StreamReader(fileLocation);
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
            var fileLocation = FileHelper.GetAbsolutePath(fileName);

            var configuration = FileHelper.GetConfiguration(delimiter, hasHeaderRecord);

            return FileHelper.GetRecords<TData>(fileLocation, configuration);
        }

        /// <summary>
        /// Creates a relative path to the file name provided.
        /// </summary>
        /// <param name="relativePath">The path to the file in the Assembly directory.</param>
        /// <param name="assemblyType">The Assembly type. The default value is null resolves `FileHelper`.</param>
        /// <returns></returns>
        public static string GetAbsolutePath(string relativePath, Type? assemblyType = null)
        {
            assemblyType ??= typeof(FileHelper);

            var dataRoot = new FileInfo(assemblyType.Assembly.Location);
            var assemblyFolderPath = dataRoot.Directory.FullName;

            return Path.Combine(assemblyFolderPath, relativePath);
        }

        public static Configuration GetConfiguration(
            string delimiter = ",",
            bool hasHeaderRecord = true)
        {
            return new Configuration
            {
                HasHeaderRecord = hasHeaderRecord,
                Delimiter = delimiter,
                BadDataFound = null,
                HeaderValidated = null,
                MissingFieldFound = null,
            };
        }
    }
}
