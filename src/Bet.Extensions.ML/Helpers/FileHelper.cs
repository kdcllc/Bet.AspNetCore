using System;
using System.IO;

namespace Bet.Extensions.ML.Helpers
{
    public static class FileHelper
    {
        /// <summary>
        /// Creates a relative path to the file name provided.
        /// </summary>
        /// <param name="relativePath">The path to the file in the Assembly directory.</param>
        /// <param name="assemblyType">The Assembly type. The default value is null resolves `FileHelper`.</param>
        /// <returns></returns>
        public static string GetAbsolutePath(string relativePath, Type assemblyType = null)
        {
            assemblyType = assemblyType ?? typeof(FileHelper);

            var dataRoot = new FileInfo(assemblyType.Assembly.Location);
            var assemblyFolderPath = dataRoot.Directory.FullName;

            return Path.Combine(assemblyFolderPath, relativePath);
        }
    }
}
