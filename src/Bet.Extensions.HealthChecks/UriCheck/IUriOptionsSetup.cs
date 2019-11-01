using System;
using System.Net;
using System.Net.Http;

namespace Bet.Extensions.HealthChecks.UriCheck
{
    public interface IUriOptionsSetup
    {
        /// <summary>
        /// The Uri to perform the healthcheck on.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        IUriOptionsSetup AddUri(Uri uri);

        /// <summary>
        /// The Uri as string to perform the healthcheck on.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        IUriOptionsSetup AddUri(string uri);

        /// <summary>
        /// Override default timeout of 10 seconds.
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        IUriOptionsSetup UseTimeout(TimeSpan timeout);

        /// <summary>
        /// Override default HttpMethod.Get.
        /// </summary>
        /// <param name="httpMethod"></param>
        /// <returns></returns>
        IUriOptionsSetup UseHttpMethod(HttpMethod httpMethod);

        /// <summary>
        /// See more for Status Codes information: <see cref="!:https://developer.mozilla.org/en-US/docs/Web/HTTP/Status"/>.
        /// </summary>summary>
        IUriOptionsSetup UseExpectHttpCodes(int min, int max);

        /// <summary>
        /// Single HttpStatus code as <see cref="HttpStatusCode"/> value.
        /// </summary>
        /// <param name="statusCode"></param>
        /// <returns></returns>
        IUriOptionsSetup UseExpectedHttpCode(HttpStatusCode statusCode);

        /// <summary>
        /// Single HttpStatus code as <see cref="int"/> value.
        /// </summary>
        /// <param name="statusCode"></param>
        /// <returns></returns>
        IUriOptionsSetup UseExpectedHttpCode(int statusCode);

        /// <summary>
        /// Add Http Custom Header to the request. It can be Authorization.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        IUriOptionsSetup AddCustomHeader(string name, string value);
    }
}
