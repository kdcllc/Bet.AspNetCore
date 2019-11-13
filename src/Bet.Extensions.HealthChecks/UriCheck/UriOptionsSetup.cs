using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;

namespace Bet.Extensions.HealthChecks.UriCheck
{
    /// <summary>
    /// The class to provide configuration for the healthcheck request.
    /// </summary>
    public class UriOptionsSetup : IUriOptionsSetup
    {
        private readonly List<(string name, string value)> _headers = new List<(string name, string value)>();

        /// <summary>
        /// Initializes a new instance of the <see cref="UriOptionsSetup"/> class.
        /// Default values for.
        /// </summary>
        /// <param name="uri"></param>
        public UriOptionsSetup(Uri uri = default!)
        {
            Uri = uri;

            // success codes
            ExpectedHttpCodes = (200, 226);

            HttpMethod = HttpMethod.Get;

            Timeout = TimeSpan.FromSeconds(10);
        }

        public HttpMethod HttpMethod { get; private set; }

        public TimeSpan Timeout { get; private set; }

        public (int min, int max) ExpectedHttpCodes { get; private set; }

        public Uri Uri { get; private set; }

        internal IEnumerable<(string name, string value)> Headers => _headers;

        /// <inheritdoc/>
        public IUriOptionsSetup AddUri(Uri uri)
        {
            Uri = uri;
            return this;
        }

        /// <inheritdoc/>
        public IUriOptionsSetup AddUri(string uri)
        {
            Uri = new Uri(uri);

            return this;
        }

        /// <inheritdoc/>
        public IUriOptionsSetup UseTimeout(TimeSpan timeout)
        {
            Timeout = timeout;
            return this;
        }

        /// <inheritdoc/>
        public IUriOptionsSetup UseHttpMethod(HttpMethod httpMethod)
        {
            HttpMethod = httpMethod;

            return this;
        }

        /// <inheritdoc/>
        public IUriOptionsSetup UseExpectHttpCodes(int min, int max)
        {
            ExpectedHttpCodes = (min, max);

            return this;
        }

        /// <inheritdoc/>
        public IUriOptionsSetup UseExpectedHttpCode(HttpStatusCode statusCode)
        {
            var code = (int)statusCode;

            ExpectedHttpCodes = (code, code);

            return this;
        }

        /// <inheritdoc/>
        public IUriOptionsSetup UseExpectedHttpCode(int statusCode)
        {
            ExpectedHttpCodes = (statusCode, statusCode);

            return this;
        }

        public IUriOptionsSetup AddCustomHeader(string name, string value)
        {
            _headers.Add((name, value));

            return this;
        }
    }
}
