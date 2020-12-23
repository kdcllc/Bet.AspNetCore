using System.Diagnostics;

namespace Xunit
{
    /// <summary>
    /// Useful attribute for stopping a test from being run
    /// see https://lostechies.com/jimmybogard/2013/06/20/run-tests-explicitly-in-xunit-net/
    /// </summary>
    public class RunnableInDebugOnlyAttribute : FactAttribute
    {
        public RunnableInDebugOnlyAttribute()
        {
            if (!Debugger.IsAttached)
            {
                Skip = "Only running in interactive mode.";
            }
        }
    }
}
