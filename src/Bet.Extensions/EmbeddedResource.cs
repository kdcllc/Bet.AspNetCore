using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Bet.Extensions
{
    public static class EmbeddedResource
    {
        /// <summary>
        /// Finds the resource file in the assembly that called this method.
        /// </summary>
        /// <param name="resourceFileName">A file name to be used in a contains search.</param>
        /// <returns>IO.Stream that the user should dispose of.</returns>
        public static byte[]? GetAsByteArrayFromCallingAssembly(string resourceFileName)
        {
            return GetAsByteArray(Assembly.GetCallingAssembly(), resourceFileName);
        }

        public static byte[]? GetAsByteArray(Assembly assembly, string resourceFileName)
        {
            var fullFileName = FindResourceByFileName(assembly, resourceFileName);
            if (!string.IsNullOrEmpty(fullFileName))
            {
                using (var embeddedStream = assembly.GetManifestResourceStream(fullFileName))
                {
                    if (embeddedStream == null)
                    {
                        return null;
                    }

                    var ba = new byte[embeddedStream.Length];
                    embeddedStream.Read(ba, 0, ba.Length);
                    return ba;
                }
            }

            return null;
        }

        /// <summary>
        /// Finds the resource file in the assembly that called this method.
        /// </summary>
        /// <param name="resourceFileName">A file name to be used in a contains search.</param>
        /// <returns>IO.Stream that the user should dispose of.</returns>
        public static Stream? GetAsStreamFromCallingAssembly(string resourceFileName)
        {
            return GetAsStream(Assembly.GetCallingAssembly(), resourceFileName);
        }

        /// <summary>
        /// Finds the resource file in the specified assembly.
        /// </summary>
        /// <param name="assembly">The assembly to pull the resource from.</param>
        /// <param name="resourceFileName">A file name to be used in a contains search.</param>
        /// <returns>IO.Stream that the user should dispose of.</returns>
        public static Stream? GetAsStream(Assembly assembly, string resourceFileName)
        {
            var fullFileName = FindResourceByFileName(assembly, resourceFileName);
            if (!string.IsNullOrEmpty(fullFileName))
            {
                return assembly.GetManifestResourceStream(fullFileName);
            }

            return null;
        }

        public static string GetAsStringFromCallingAssemly(string resourceFileName)
        {
            return GetAsString(Assembly.GetCallingAssembly(), resourceFileName);
        }

        public static string GetAsString(Assembly assembly, string resourceFileName)
        {
            using (var stream = GetAsStream(assembly, resourceFileName))
            using (var reader = new StreamReader(stream))
            {
                var result = reader.ReadToEnd();
                return result;
            }
        }

        /// <summary>
        /// Persist the embedded resource to disk from calling assembly.
        /// </summary>
        /// <param name="resourceFileName">file name including extension.</param>
        /// <param name="pathToSave">full path to save file.</param>
        /// <returns>full path to file including file name.</returns>
        public static string SaveToDisk(string resourceFileName, string pathToSave)
        {
            return SaveToDisk(Assembly.GetCallingAssembly(), resourceFileName, pathToSave);
        }

        /// <summary>
        /// Persist the embedded resource to disk.
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="resourceFileName">file name including extension.</param>
        /// <param name="pathToSave">full path to save file.</param>
        /// <returns>full path to file including file name.</returns>
        public static string SaveToDisk(Assembly assembly, string resourceFileName, string pathToSave)
        {
            var fullFilePath = Path.Combine(pathToSave, resourceFileName);

            using (var stream = GetAsStream(assembly, resourceFileName))
            using (var fileStream = File.Create(fullFilePath))
            {
                stream.CopyTo(fileStream);
            }

            return fullFilePath;
        }

        private static string FindResourceByFileName(Assembly assembly, string fileName)
        {
            var result = string.Empty;
            if (!string.IsNullOrWhiteSpace(fileName))
            {
                result = assembly.GetManifestResourceNames()
                    .FirstOrDefault(n => n.ToLower().Contains(fileName.Replace("/", ".").ToLower()));
            }

            return result;
        }
    }
}
