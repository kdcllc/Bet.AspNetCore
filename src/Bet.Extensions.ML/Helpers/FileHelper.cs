﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;

using CsvHelper;
using CsvHelper.Configuration;

namespace Bet.Extensions.ML.Helpers
{
    public static class FileHelper
    {
        public static List<TData> GetRecords<TData>(
            string fileLocation,
            CsvConfiguration configuration) where TData : class
        {
            using var reader = new StreamReader(fileLocation);
            using (var csv = new CsvReader(reader, configuration))
            {
                var records = csv.GetRecords<TData>();
                return records.ToList();
            }
        }

        public static List<TData> GetRecords<TData>(
           Stream stream,
           CsvConfiguration configuration) where TData : class
        {
            using var reader = new StreamReader(stream);
            configuration.BadDataFound = null;
            using var csv = new CsvReader(reader, configuration);
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
            var fileLocation = GetAbsolutePath(fileName);

            var configuration = GetConfiguration(delimiter, hasHeaderRecord);

            return GetRecords<TData>(fileLocation, configuration);
        }

        public static void SaveFile(byte[] data, string fileName)
        {
            var fileLocation = GetAbsolutePath(fileName);
            File.WriteAllBytes(fileLocation, data);
        }

        public static List<TData> GetRecordsFromZipFile<TData>(
            string fileName,
            string delimiter = ",",
            bool hasHeaderRecord = true)
        {
            var fileLocation = GetAbsolutePath(fileName);
            var configuration = GetConfiguration(delimiter, hasHeaderRecord);

            using var file = File.OpenRead(fileLocation);
            return GetRecordsFromZipFile<TData>(file, configuration);
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

        public static CsvConfiguration GetConfiguration(
            string delimiter = ",",
            bool hasHeaderRecord = true)
        {
            return new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = hasHeaderRecord,
                Delimiter = delimiter,
                BadDataFound = null,
                HeaderValidated = null,
                MissingFieldFound = null,
            };
        }

        public static List<TData> GetRecordsFromZipFile<TData>(
            byte[] zipFile,
            CsvConfiguration configuration)
        {
            var stream = new MemoryStream();
            stream.Write(zipFile, 0, zipFile.Length);
            stream.Position = 0;

            return GetRecordsFromZipFile<TData>(stream, configuration);
        }

        public static List<TData> GetRecordsFromZipFile<TData>(
            Stream zipFile,
            CsvConfiguration configuration)
        {
            var result = new List<TData>();

            using var archive = new ZipArchive(zipFile);

            configuration.HeaderValidated = null;
            configuration.MissingFieldFound = null;

            foreach (var entry in archive.Entries)
            {
                using var unzippedFile = entry.Open(); // assume that a single file is in the zip.

                using var reader = new StreamReader(unzippedFile);
                using var csv = new CsvReader(reader, configuration);

                var records = csv.GetRecords<TData>().ToList();

                if (records != null)
                {
                    result.AddRange(records);
                }
            }

            archive.Dispose();

            return result;
        }

        public static byte[] GetZipFileFromRecords<T>(
            IEnumerable<T> records,
            string delimiter = ",",
            bool hasHeaderRecord = true)
        {
            using var zipStream = new MemoryStream();
            using var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, true);

            var zipItem = archive.CreateEntry("records.csv", CompressionLevel.Optimal);

            using var zipItemStream = zipItem.Open();

            var csvRecords = GetStreamFromRecords(records, GetConfiguration(delimiter, hasHeaderRecord));
            zipItemStream.Write(csvRecords, 0, csvRecords.Length);

            archive.Dispose();

            var bytes = zipStream.ToArray();

            return bytes;
        }

        private static byte[] GetStreamFromRecords<T>(
            IEnumerable<T> records,
            CsvConfiguration configuration)
        {
            using var stream = new MemoryStream();
            using var streamWrite = new StreamWriter(stream);
            using var csvWriter = new CsvWriter(streamWrite, configuration);

            // 1. write records to stream
            // https://github.com/JoshClose/CsvHelper/blob/f68a5a1cdecb4e4f7b2a536b0ce482deb95acda5/src/CsvHelper.Website/input/migration/v20/index.md#automap
            csvWriter.Context.AutoMap<T>();

            csvWriter.WriteRecords(records);
            streamWrite.Flush();
            stream.Seek(0, SeekOrigin.Begin);

            var bytes = stream.ToArray();

            return bytes;
        }
    }
}
