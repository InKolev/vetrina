using System.Threading.Tasks;

namespace Vetrina.Client.Services
{
    public interface IClipboardService
    {
        ValueTask<string> ReadTextAsync();

        ValueTask WriteTextAsync(string text);
    }
}