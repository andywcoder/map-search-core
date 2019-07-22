using Santolibre.Map.Search.Lib.Models;

namespace Santolibre.Map.Search.Lib.Repositories
{
    public interface ITranslationCacheRepository
    {
        TranslationCache GetTranslationCache(string id);
        void SaveTranslationCache(TranslationCache translationCache);
    }
}
