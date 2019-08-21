// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Runtime.Serialization;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace AppAuthentication.Models
{
    /// <summary>
    /// Contains access token and other results from a token request call.
    /// </summary>
    internal class AppAuthenticationResult
    {
        /// <summary>
        /// The access token returned from the token request.
        /// </summary>
        [DataMember]
        public string AccessToken { get; private set; }

        /// <summary>
        /// The time when the access token expires.
        /// </summary>
        [DataMember]
        public DateTimeOffset ExpiresOn { get; private set; }

        /// <summary>
        /// The Resource URI of the receiving web service.
        /// </summary>
        [DataMember]
        public string Resource { get; private set; }

        /// <summary>
        /// Indicates the token type value.
        /// </summary>
        [DataMember]
        public string TokenType { get; private set; }

        /// <summary>
        /// Return true when access token is near expiration.
        /// </summary>
        /// <returns></returns>
        internal bool IsNearExpiry()
        {
            // If the expiration time is within the next 5 minutes, the token is about to expire
            return ExpiresOn < DateTimeOffset.UtcNow.AddMinutes(5);
        }

        internal static AppAuthenticationResult Create(TokenResponse response, TokenResponse.DateFormat dateFormat)
        {
            if (response == null)
            {
                throw new ArgumentNullException(nameof(response));
            }

            var expiresOnString = response.ExpiresOn ?? response.ExpiresOn2;
            var expiresOn = DateTimeOffset.MinValue;

            switch (dateFormat)
            {
                case TokenResponse.DateFormat.DateTimeString:
                    expiresOn = DateTimeOffset.Parse(expiresOnString);
                    break;
                case TokenResponse.DateFormat.Unix:
                    var seconds = double.Parse(expiresOnString);
                    expiresOn = Models.AccessToken.UnixTimeEpoch.AddSeconds(seconds);
                    break;
            }

            var result = new AppAuthenticationResult()
            {
                AccessToken = response.AccessToken ?? response.AccessToken2,
                ExpiresOn = expiresOn,
                Resource = response.Resource,
                TokenType = response.TokenType ?? response.TokenType2
            };

            return result;
        }

        internal static AppAuthenticationResult Create(AuthenticationResult authResult)
        {
            if (authResult == null)
            {
                throw new ArgumentNullException(nameof(authResult));
            }

            var result = new AppAuthenticationResult()
            {
                AccessToken = authResult.AccessToken,
                ExpiresOn = authResult.ExpiresOn
            };

            return result;
        }

        // For unit testing
        internal static AppAuthenticationResult Create(string accessToken)
        {
            if (string.IsNullOrEmpty(accessToken))
            {
                throw new ArgumentNullException(nameof(accessToken));
            }

            var tokenObj = Models.AccessToken.Parse(accessToken);

            var result = new AppAuthenticationResult
            {
                AccessToken = accessToken,
                ExpiresOn = Models.AccessToken.UnixTimeEpoch.AddSeconds(tokenObj.ExpiryTime)
            };

            return result;
        }
    }
}
