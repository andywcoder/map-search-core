using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Santolibre.Map.Search.Lib.Models;
using Santolibre.Map.Search.Lib.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Santolibre.Map.Search.Lib.Services
{
    public class TranslationService : ITranslationService
    {
        private readonly MemoryCache _memoryCache;
        private readonly ITranslationRepository _translationRepository;
        private readonly ILogger<TranslationService> _logger;

        public TranslationService(ITranslationRepository translationRepository, ILogger<TranslationService> logger)
        {
            _memoryCache = new MemoryCache(new MemoryCacheOptions());
            _translationRepository = translationRepository;
            _logger = logger;
        }

        public void PopulateCache(List<(Language From, Language To, string Term, string TranslatedTerm)> terms)
        {
            foreach (var term in terms)
            {
                var key = $"{term.From}_{term.To}_{term.Term.ToLower()}";
                _memoryCache.Set(key, term.TranslatedTerm);
            }
        }

        public List<(string Term, string TranslatedTerm)> GetTranslation(Language from, Language to, List<string> terms, bool selectInconclusive)
        {
            var translatedTerms = new List<(string, string)>();
            var termsToTranslate = new List<string>();

            _logger.LogDebug($"Checking translation cache");
            foreach (var term in terms)
            {
                var key = $"{from}_{to}_{term.ToLower()}";

                if (_memoryCache.TryGetValue(key, out string translatedTerm))
                {
                    _logger.LogDebug($"Term '{term}' is cached");
                    translatedTerms.Add((term, translatedTerm));
                }
                else
                {
                    _logger.LogDebug($"Term '{term}' is not cached, adding it to the translation list");
                    termsToTranslate.Add(term);
                }
            }

            var translationResults = _translationRepository.GetTranslationAsync(from, to, termsToTranslate).Result;
            foreach (var translationResult in translationResults)
            {
                var term = translationResult.NormalizedSource;

                _logger.LogDebug($"Processing term '{term}'");

                string translatedTerm;
                if (selectInconclusive && translationResult.Translations.Length > 1)
                {
                    _logger.LogInformation($"Term translation for '{term}' is inconclusive, please select one of the following translations:");
                    for (var i = 0; i < translationResult.Translations.Length; i++)
                    {
                        _logger.LogInformation($"[{i}] {translationResult.Translations[i].NormalizedTarget.ToLower()}");
                    }

                    string pressedKey;
                    int termIndex;
                    do
                    {
                        pressedKey = Console.ReadKey().KeyChar.ToString();
                        Console.WriteLine();
                    }
                    while (int.TryParse(pressedKey, out termIndex) && termIndex > translationResult.Translations.Length);
                    translatedTerm = translationResult.Translations[termIndex].NormalizedTarget.ToLower();
                    _logger.LogDebug($"Selected '{translatedTerm}' as translation");
                }
                else if (translationResult.Translations.Length > 0)
                {
                    translatedTerm = translationResult.Translations.First().NormalizedTarget.ToLower();
                    _logger.LogDebug($"Selected first translation '{translatedTerm}'");
                }
                else
                {
                    _logger.LogDebug($"No translation found");
                    translatedTerm = term;
                }

                var key = $"{from}_{to}_{term.ToLower()}";

                translatedTerms.Add((term, translatedTerm));
                _memoryCache.Set(key, translatedTerm);
            }

            return translatedTerms;
        }
    }
}
