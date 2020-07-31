using System.Collections;
using System.Collections.Generic;

namespace Bet.AnalyticsEngine
{
    public class AnalyticsEngineOptions
    {
        public string DatabasePath { get; set; } = string.Empty;

        public string FileName { get; set; } = $"{nameof(AnalyticsEngine)}.db";

        public string Database => $"Filename={DatabasePath}{FileName}";

        public IEnumerable<string> Exclude { get; set; } = new List<string> { "/healthy", "/liveness", "/favicon.ico" };
    }
}
