using System.Threading;

namespace Vetrina.Server.Abstractions
{
    public interface IJob
    {
        void Execute(CancellationToken cancellationToken);
    }
}