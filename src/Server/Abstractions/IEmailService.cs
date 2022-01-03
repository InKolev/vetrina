using System.Threading;
using System.Threading.Tasks;
using Vetrina.Server.Models;

namespace Vetrina.Server.Abstractions
{
    public interface IEmailService
    {
        Task<SendEmailResponse> SendEmailAsync(
            SendEmailRequest sendEmailRequest, 
            CancellationToken cancellationToken);
    }
}
