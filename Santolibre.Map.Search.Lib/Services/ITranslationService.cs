using Santolibre.Map.Search.Lib.Models;
using System.Collections.Generic;

namespace Santolibre.Map.Search.Lib.Services
{
    public interface ITranslationService
    {
        void PopulateCache(List<(Language From, Language To, string Term, string TranslatedTerm)> terms);
        List<(string Term, string TranslatedTerm)> GetTranslation(Language from, Language to, List<string> terms, bool selectInconclusive);
    }
}
