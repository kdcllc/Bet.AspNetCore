// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Runtime.Serialization;

namespace AppAuthentication.VisualStudio
{
    /// <summary>
    /// Used to de-serialize the Visual Studio token provider file.
    /// </summary>
    [DataContract]
    internal class VisualStudioTokenProvider
    {
        [DataMember(Name = "Path", IsRequired = true)]
        public string Path;

        [DataMember(Name = "Arguments", IsRequired = false)]
        public List<string> Arguments;

        [DataMember(Name = "Preference", IsRequired = true)]
        public int Preference;
    }
}
