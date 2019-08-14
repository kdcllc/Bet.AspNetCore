// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace AppAuthentication.Models
{
    /// <summary>
    /// Helper class that handles HTTP Bearer challenges.
    /// </summary>
    internal class HttpBearerChallenge
    {
        private const string AuthorizationParameter = "authorization";
        private const string AuthorizationUriParameter = "authorization_uri";
        private const string ResourceParameter = "resource";
        private const string ScopeParameter = "scope";
        private const string Bearer = "Bearer";

        public string AuthorizationServer { get; private set; }

        public string Resource { get; private set; }

        public string Scope { get; private set; }

        /// <summary>
        /// Parses an HTTP Bearer challenge from Key Vault.
        /// </summary>
        /// <param name="challenge">The value of the WWW-Authenticate header to parse.</param>
        /// <returns></returns>
        internal static HttpBearerChallenge Parse(string challenge)
        {
            if (!ValidateChallenge(challenge))
            {
                return null;
            }

            var parameters = GetChallengeParameters(challenge);

            string authorization = null;
            string resource = null;
            string scope = null;

            if (parameters.ContainsKey(AuthorizationParameter))
            {
                authorization = parameters[AuthorizationParameter];
            }
            else if (parameters.ContainsKey(AuthorizationUriParameter))
            {
                authorization = parameters[AuthorizationUriParameter];
            }

            if (parameters.ContainsKey(ResourceParameter))
            {
                resource = parameters[ResourceParameter];
            }

            if (parameters.ContainsKey(ScopeParameter))
            {
                scope = parameters[ScopeParameter];
            }

            // scope is not a required parameter
            if (authorization == null || resource == null)
            {
                return null;
            }

            return new HttpBearerChallenge(authorization, resource, scope);
        }

        private static bool ValidateChallenge(string challenge)
        {
            if (string.IsNullOrEmpty(challenge))
            {
                return false;
            }

            if (!challenge.Trim().StartsWith(Bearer + " "))
            {
                return false;
            }

            return true;
        }

        private static Dictionary<string, string> GetChallengeParameters(string challenge)
        {
            // first, trim the challenge's Bearer prefix
            challenge = challenge.Trim().Substring(Bearer.Length + 1);

            // then parse the key-value pairs into a dictionary
            var parameters = new Dictionary<string, string>();
            var keyValuePairs = challenge.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            if (keyValuePairs != null && keyValuePairs.Length > 0)
            {
                for (var i = 0; i < keyValuePairs.Length; i++)
                {
                    var keyValuePair = keyValuePairs[i].Split('=');

                    if (keyValuePair.Length == 2)
                    {
                        var key = keyValuePair[0].Trim().Trim('"');
                        var value = keyValuePair[1].Trim().Trim('"');

                        if (!string.IsNullOrEmpty(key))
                        {
                            parameters[key] = value;
                        }
                    }
                }
            }

            return parameters;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpBearerChallenge"/> class.
        /// Create instance of HTTP Bearer Challenge.
        /// </summary>
        /// <param name="authorization"></param>
        /// <param name="resource"></param>
        /// <param name="scope"></param>
        private HttpBearerChallenge(string authorization, string resource, string scope)
        {
            AuthorizationServer = authorization;
            Resource = resource;
            Scope = scope;
        }
    }
}
