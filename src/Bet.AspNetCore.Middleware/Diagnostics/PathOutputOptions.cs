using System;

namespace Bet.AspNetCore.Middleware.Diagnostics
{
    /// <summary>
    /// Path options to display the output of the Registered Services within DI container.
    /// </summary>
    [Flags]
    public enum PathOutputOptions
    {
        Html,
        Json
    }
}
