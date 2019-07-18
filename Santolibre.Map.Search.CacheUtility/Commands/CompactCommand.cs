using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Santolibre.Map.Search.Lib.Services;

namespace Santolibre.Map.Search.CacheUtility.Commands
{
    public class CompactCommand
    {
        private readonly IMaintenanceService _maintenanceService;
        private readonly ILogger<CompactCommand> _logger;

        public static void Configure(CommandLineApplication command)
        {
            command.Description = "Compact database";
            command.HelpOption("-?|-h|--help");

            command.OnExecute(() =>
            {
                command.GetRequiredService<CompactCommand>().Run();
                return 0;
            });
        }

        public CompactCommand(IMaintenanceService maintenanceService, ILogger<CompactCommand> logger)
        {
            _maintenanceService = maintenanceService;
            _logger = logger;
        }

        public void Run()
        {
            _maintenanceService.CompactPointsOfInterest();
        }
    }
}
