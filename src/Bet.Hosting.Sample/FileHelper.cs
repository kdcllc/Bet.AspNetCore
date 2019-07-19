using System.IO;

namespace Bet.Hosting.Sample.Services
{
    public static class FileHelper
    {
        public static string GetAbsolutePath(string relativePath)
        {
            var _dataRoot = new FileInfo(typeof(Program).Assembly.Location);
            var assemblyFolderPath = _dataRoot.Directory.FullName;

            return Path.Combine(assemblyFolderPath, relativePath);
        }
    }
}
