using System;
using System.Collections.Generic;

namespace Bet.Extensions.Options
{
    /// <summary>
    /// Implement this interface in order to create an instances of validations for AspNetCore of Generic Host.
    /// </summary>
    public interface IValidationFilter
    {
        /// <summary>
        /// Contains Configuration validations types and name of the section were it was loaded from.
        /// </summary>
        IList<(Type type, string sectionName)> OptionsTypes { get; }
    }
}
