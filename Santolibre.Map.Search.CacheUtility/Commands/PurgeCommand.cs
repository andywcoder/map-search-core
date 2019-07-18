using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Santolibre.Map.Search.Lib.Services;

namespace Santolibre.Map.Search.CacheUtility.Commands
{
    public class PurgeCommand
    {
        private readonly IMaintenanceService _maintenanceService;
        private readonly ILogger<PurgeCommand> _logger;

        public static void Configure(CommandLineApplication command)
        {
            command.Description = "Purge data";
            command.HelpOption("-?|-h|--help");
            var olderThanInDays = command.Option<int>("-o|--older-than <DAYS>", "[Required] Specify max age", CommandOptionType.SingleValue).IsRequired();

            command.OnExecute(() =>
            {
                command.GetRequiredService<PurgeCommand>().Run(olderThanInDays.ParsedValue);
                return 0;
            });
        }

        public PurgeCommand(IMaintenanceService maintenanceService, ILogger<PurgeCommand> logger)
        {
            _maintenanceService = maintenanceService;
            _logger = logger;
        }

        public void Run(int olderThanInDays)
        {
            _maintenanceService.RemoveOldPointsOfInterest(olderThanInDays);
        }
    }
}
