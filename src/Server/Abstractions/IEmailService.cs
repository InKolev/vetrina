using System.Threading;
using System.Threading.Tasks;
using Vetrina.Shared;
using Vetrina.Shared.Models;

namespace Vetrina.Server.Abstractions
{
    public interface IEmailService
    {
        Task<SendEmailResponse> SendEmailAsync(
            SendEmailRequest sendEmailRequest, 
            CancellationToken cancellationToken);
    }
}
