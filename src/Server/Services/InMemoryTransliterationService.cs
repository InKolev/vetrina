using System;
using System.Threading.Tasks;
using NickBuhro.Translit;
using Vetrina.Server.Abstractions;
using Vetrina.Shared;

namespace Vetrina.Server.Services
{
    public class InMemoryTransliterationService : ITransliterationService
    {
        public Task<string> CyrillicToLatinAsync(string cyrillicText, LanguageHint language)
        {
            if (language == LanguageHint.Bulgarian)
            {
                var latinText = Transliteration.CyrillicToLatin(cyrillicText, Language.Bulgarian);

                return Task.FromResult(latinText);
            }

            throw new NotSupportedException(
                $"Language {language} is currently not supported by the {nameof(InMemoryTransliterationService)}.{nameof(CyrillicToLatinAsync)}.");
        }

        public Task<string> LatinToCyrillicAsync(string latinText, LanguageHint language)
        {
            if (language == LanguageHint.Bulgarian)
            {
                var cyrillicText = Transliteration.LatinToCyrillic(latinText, Language.Bulgarian);

                return Task.FromResult(cyrillicText);
            }

            throw new NotSupportedException(
                $"Language {language} is currently not supported by the {nameof(InMemoryTransliterationService)}.{nameof(LatinToCyrillicAsync)}.");
        }
    }
}