using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Santolibre.Map.Search.Lib.Models;
using Santolibre.Map.Search.Lib.Services;
using System;
using System.Collections.Generic;
using System.IO;

namespace Santolibre.Map.Search.CacheUtility.Commands
{
    public class UpdateTranslationCacheCommand
    {
        private readonly IMaintenanceService _maintenanceService;
        private readonly ITranslationService _translationService;
        private readonly ILogger<UpdateTranslationCacheCommand> _logger;

        public static void Configure(CommandLineApplication command)
        {
            command.Description = "Update translation cache";
            command.HelpOption("-?|-h|--help");
            var from = command.Option<Language>("-f|--from <LANGUAGE>", "Source language", CommandOptionType.SingleValue);
            var to = command.Option<Language>("-t|--to <LANGUAGE>", "Destination language", CommandOptionType.SingleValue);
            var mappingFile = command.Option("-m|--mapping-file <MAPPING_FILE>", "Mapping CSV file", CommandOptionType.SingleValue);
            var selectInconclusive = command.Option("-s|--select-inconclusive", "Inconculsive terms have to be selected manually", CommandOptionType.NoValue);

            command.OnExecute(() =>
            {
                command.GetRequiredService<UpdateTranslationCacheCommand>().Run(from.ParsedValue, to.ParsedValue, mappingFile.Value(), selectInconclusive.HasValue());
                return 0;
            });
        }

        public UpdateTranslationCacheCommand(IMaintenanceService maintenanceService, ITranslationService translationService, ILogger<UpdateTranslationCacheCommand> logger)
        {
            _maintenanceService = maintenanceService;
            _translationService = translationService;
            _logger = logger;
        }

        public void Run(Language from, Language to, string mappingFile, bool selectInconclusive)
        {
            var mappingEntries = File.ReadAllText(mappingFile).ToLower().Split(Environment.NewLine);
            foreach (var mappingEntry in mappingEntries)
            {
                var keyValue = mappingEntry.Split(';');
                _translationService.PopulateCache(new List<(Language, Language, string, string)>
                {
                    (Language.EN, Language.DE, keyValue[0], keyValue[1])
                });
            }

            _maintenanceService.UpdateTranslationCache(from, to, selectInconclusive);
        }
    }
}
