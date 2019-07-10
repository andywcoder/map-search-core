using Microsoft.Extensions.Logging;
using Santolibre.Map.Search.Lib.Services;
using System;
using System.Linq;

namespace Santolibre.Map.Search.CacheUtility
{
    public class App
    {
        private readonly IMaintenanceService _maintenanceService;
        private readonly ILogger<App> _logger;

        public App(IMaintenanceService maintenanceService, ILogger<App> logger)
        {
            _maintenanceService = maintenanceService;
            _logger = logger;
        }

        public void Run(string[] args)
        {
            _logger.LogDebug($"Parsing command arguments {string.Join(", ", args)}");
            var importFilename = args.Any(x => x.StartsWith("--import=")) ? args.FirstOrDefault(x => x.StartsWith("--import=")).Replace("--import=", "") : null;
            var removeOlderThan = args.Any(x => x.StartsWith("--remove-older-than=")) ? int.Parse(args.FirstOrDefault(x => x.StartsWith("--remove-older-than=")).Replace("--remove-older-than=", "")) : (int?)null;
            var analyzeIndexTerms = args.Any(x => x.StartsWith("--analyze-index-terms"));
            var compactDatabase = args.Any(x => x.StartsWith("--compact-database"));

            if (!string.IsNullOrEmpty(importFilename))
            {
                _maintenanceService.ImportPointsOfInterest(importFilename);
            }
            else if (removeOlderThan.HasValue)
            {
                _maintenanceService.RemoveOldPointsOfInterest(removeOlderThan.Value);
            }
            else if (analyzeIndexTerms)
            {
                _maintenanceService.AnalyzeIndexTerms();
            }
            else if (compactDatabase)
            {
                _maintenanceService.CompactPointsOfInterest();
            }
            else
            {
                Console.WriteLine("Usage information");
                Console.WriteLine();
                Console.WriteLine("--import=[filename]");
                Console.WriteLine("--remove-older-than=[days]");
                Console.WriteLine("--analyze-index-terms");
                Console.WriteLine("--compact-database");
            }
        }
    }
}
