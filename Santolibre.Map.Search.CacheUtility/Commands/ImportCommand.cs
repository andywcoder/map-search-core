using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Santolibre.Map.Search.Lib.Services;

namespace Santolibre.Map.Search.CacheUtility.Commands
{
    public class ImportCommand
    {
        private readonly IMaintenanceService _maintenanceService;
        private readonly ILogger<ImportCommand> _logger;

        public static void Configure(CommandLineApplication command)
        {
            command.Description = "Import data";
            command.HelpOption("-?|-h|--help");
            var osmPbfFile = command.Option("-f|--osm-pbf-file <OSM_PBF_FILE>", "[Required] OSM.PBF file to import", CommandOptionType.SingleValue).IsRequired();

            command.OnExecute(() =>
            {
                command.GetRequiredService<ImportCommand>().Run(osmPbfFile.Value());
                return 0;
            });
        }

        public ImportCommand(IMaintenanceService maintenanceService, ILogger<ImportCommand> logger)
        {
            _maintenanceService = maintenanceService;
            _logger = logger;
        }

        public void Run(string osmPbfFile)
        {
            _maintenanceService.ImportPointsOfInterest(osmPbfFile);
        }
    }
}
