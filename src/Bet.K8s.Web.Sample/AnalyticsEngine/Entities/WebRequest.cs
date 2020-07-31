using System;
using System.ComponentModel.DataAnnotations;

namespace Bet.AnalyticsEngine
{
    public class WebRequest
    {
        public long Id { get; set; }

        public DateTime Timestamp { get; set; }

        [MaxLength(32)]
        public string Identity { get; set; }

        [MaxLength(32)]
        public string RemoteIpAddress { get; set; }

        [MaxLength(16)]
        public string Method { get; set; }

        [MaxLength(1024)]
        public string Path { get; set; }

        [MaxLength(512)]
        public string UserAgent { get; set; }

        [MaxLength(1024)]
        public string Referer { get; set; }

        public bool IsWebSocket { get; set; }

        [MaxLength(1024)]
        public string IpCode { get; set; }
    }
}
