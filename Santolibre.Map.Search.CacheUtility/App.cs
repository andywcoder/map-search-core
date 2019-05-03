using Microsoft.Extensions.Logging;
using Santolibre.Map.Search.Lib.Services;
using System;
using System.Linq;

namespace Santolibre.Map.Search.CacheUtility
{
    public class App
    {
        private readonly ISearchService _searchService;
        private readonly ILogger<App> _logger;

        public App(ISearchService searchService, ILogger<App> logger)
        {
            _searchService = searchService;
            _logger = logger;
        }

        public void Run(string[] args)
        {
            _logger.LogDebug($"Parsing command arguments {string.Join(", ", args)}");
            var importFilename = args.Any(x => x.StartsWith("--import=")) ? args.FirstOrDefault(x => x.StartsWith("--import=")).Replace("--import=", "") : null;
            var removeOlderThan = args.Any(x => x.StartsWith("--remove-older-than=")) ? int.Parse(args.FirstOrDefault(x => x.StartsWith("--remove-older-than=")).Replace("--remove-older-than=", "")) : (int?)null;
            var updateSuggestions = args.Any(x => x.StartsWith("--update-suggestions"));
            var compactDatabase = args.Any(x => x.StartsWith("--compact-database"));

            if (!string.IsNullOrEmpty(importFilename))
            {
                _searchService.ImportPointsOfInterest(importFilename);
            }
            else if (removeOlderThan.HasValue)
            {
                _searchService.RemoveOldPointsOfInterest(removeOlderThan.Value);
            }
            else if (updateSuggestions)
            {
                _searchService.UpdateSuggestions();
            }
            else if (compactDatabase)
            {
                _searchService.CompactPointsOfInterest();
            }
            else
            {
                Console.WriteLine("Usage information");
                Console.WriteLine();
                Console.WriteLine("--import=[filename]");
                Console.WriteLine("--remove-older-than=[days]");
                Console.WriteLine("--compact-database");
            }
        }
    }
}
