using System.Threading.Tasks;
using Vetrina.Shared;

namespace Vetrina.Server.Abstractions
{
    public interface ITransliterationService
    {
        Task<string> CyrillicToLatinAsync(string cyrillicText, LanguageHint language);

        Task<string> LatinToCyrillicAsync(string latinText, LanguageHint language);
    }
}