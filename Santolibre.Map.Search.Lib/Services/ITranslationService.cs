using System.Collections.Generic;

namespace Santolibre.Map.Search.Lib.Services
{
    public interface ITranslationService
    {
        void PopulateCache(List<(string From, string To, string Term, string TranslatedTerm)> terms);
        List<string> GetTranslation(string from, string to, List<string> terms);
    }
}
