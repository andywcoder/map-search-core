using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Santolibre.Map.Search.Lib.Models;
using Santolibre.Map.Search.Lib.Services;

namespace Santolibre.Map.Search.CacheUtility.Commands
{
    public class UpdateTranslationCacheCommand
    {
        private readonly IMaintenanceService _maintenanceService;
        private readonly ILogger<UpdateTranslationCacheCommand> _logger;

        public static void Configure(CommandLineApplication command)
        {
            command.Description = "Update translation cache";
            command.HelpOption("-?|-h|--help");
            var from = command.Option<Language>("-f|--from <LANGUAGE>", "Source language)", CommandOptionType.SingleValue);
            var to = command.Option<Language>("-t|--to <LANGUAGE>", "Destination language)", CommandOptionType.SingleValue);

            command.OnExecute(() =>
            {
                command.GetRequiredService<UpdateTranslationCacheCommand>().Run(from.ParsedValue, to.ParsedValue);
                return 0;
            });
        }

        public UpdateTranslationCacheCommand(IMaintenanceService maintenanceService, ILogger<UpdateTranslationCacheCommand> logger)
        {
            _maintenanceService = maintenanceService;
            _logger = logger;
        }

        public void Run(Language from, Language to)
        {
            _maintenanceService.UpdateTranslationCache(from, to);
        }
    }
}
