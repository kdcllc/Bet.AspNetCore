// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

using AppAuthentication.Helpers;

namespace AppAuthentication.VisualStudio
{
    [DataContract]
    internal class VisualStudioTokenProviderFile
    {
        [DataMember(Name = "TokenProviders")]
        public List<VisualStudioTokenProvider> TokenProviders;

        private const string FormatExceptionMessage = "VisualStudio Token Provider File is not in the expected format.";

        public static VisualStudioTokenProviderFile Parse(string fileContents)
        {
            try
            {
                var visualStudioTokenProviderFile = JsonHelper.Deserialize<VisualStudioTokenProviderFile>(Encoding.UTF8.GetBytes(fileContents));

                // Order the providers, so that the latest one is tried first.
                visualStudioTokenProviderFile.TokenProviders =
                    visualStudioTokenProviderFile.TokenProviders.OrderByDescending(p => p.Preference).ToList();

                return visualStudioTokenProviderFile;
            }
            catch (Exception exp)
            {
                throw new FormatException($"{FormatExceptionMessage} Exception Message: {exp.Message}");
            }
        }
    }
}
