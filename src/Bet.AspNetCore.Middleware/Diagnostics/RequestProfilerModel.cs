using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Bet.AspNetCore.Middleware.Diagnostics
{
    /// <summary>
    /// Request Response Profile model.
    /// </summary>
    public class RequestProfilerModel
    {
        public DateTimeOffset RequestTime { get; set; }

        public HttpContext Context { get; set; }

        public string Request { get; set; } = string.Empty;

        public string Response { get; set; } = string.Empty;

        public DateTimeOffset ResponseTime { get; set; }
    }
}
