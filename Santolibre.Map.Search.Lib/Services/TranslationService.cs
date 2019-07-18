using Microsoft.Extensions.Caching.Memory;
using Santolibre.Map.Search.Lib.Repositories;
using System.Collections.Generic;
using System.Linq;

namespace Santolibre.Map.Search.Lib.Services
{
    public class TranslationService : ITranslationService
    {
        private readonly MemoryCache _memoryCache;
        private readonly ITranslationRepository _translationRepository;

        public TranslationService(ITranslationRepository translationRepository)
        {
            _memoryCache = new MemoryCache(new MemoryCacheOptions());
            _translationRepository = translationRepository;
        }

        public void PopulateCache(List<(string From, string To, string Term, string TranslatedTerm)> terms)
        {
            foreach (var term in terms)
            {
                var key = $"{term.From}_{term.To}_{term.Term.ToLower()}";
                _memoryCache.Set(key, term.TranslatedTerm);
            }
        }

        public List<string> GetTranslation(string from, string to, List<string> terms)
        {
            var translatedTerms = new List<string>();
            var termsToTranslate = new List<string>();

            foreach (var term in terms)
            {
                var key = $"{from}_{to}_{term.ToLower()}";

                if (_memoryCache.TryGetValue(key, out string translatedTerm))
                {
                    translatedTerms.Add(translatedTerm);
                }
                else
                {
                    termsToTranslate.Add(term);
                }
            }

            var translationResults = _translationRepository.GetTranslationAsync(from, to, termsToTranslate).Result;
            foreach (var translationResult in translationResults)
            {
                var term = translationResult.NormalizedSource;
                var translatedTerm = translationResult.Translations.First().NormalizedTarget.ToLower();
                var key = $"{from}_{to}_{term.ToLower()}";

                translatedTerms.Add(translatedTerm);
                _memoryCache.Set(key, translatedTerm);
            }

            return translatedTerms;
        }
    }
}
