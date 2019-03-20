using System;
using System.Collections.Generic;

namespace Bet.Extensions.Options
{
    public interface IValidationFilter
    {
        IList<(Type type, string sectionName)> OptionsTypes { get; }
    }
}
