using System.IO;

namespace Bet.Hosting.Sample.Services
{
    public class ModelPathService
    {
        public string SpamModelPath => GetAbsolutePath("SpamModel.zip");

        public string SentimentModelPath => GetAbsolutePath("SentimentModel.zip");

        public static string GetAbsolutePath(string relativePath)
        {
            var _dataRoot = new FileInfo(typeof(Program).Assembly.Location);
            var assemblyFolderPath = _dataRoot.Directory.FullName;

            return Path.Combine(assemblyFolderPath, relativePath);
        }
    }
}
