using System.Diagnostics;
using System.Threading.Tasks;

namespace AppAuthentication
{
    /// <summary>
    /// Interface that helps mock invoking a process and getting the result from standard output and error streams.
    /// </summary>
    internal interface IProcessManager
    {
        Task<string> ExecuteAsync(Process process);
    }
}
