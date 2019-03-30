using System.Threading.Tasks;

namespace Bet.CleanArchitecture.Core.Interfaces
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string message);
    }
}
