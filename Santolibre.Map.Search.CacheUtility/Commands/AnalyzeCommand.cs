using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Santolibre.Map.Search.Lib.Services;

namespace Santolibre.Map.Search.CacheUtility.Commands
{
    public class AnalyzeCommand
    {
        private readonly IMaintenanceService _maintenanceService;
        private readonly ILogger<AnalyzeCommand> _logger;

        public static void Configure(CommandLineApplication command)
        {
            command.Description = "Analyze database";
            command.HelpOption("-?|-h|--help");
            var option = command.Option<AnalyzeOption>("-o|--option <OPTION>", "Analysis option (IndexTerms = Default)", CommandOptionType.SingleValue);

            command.OnExecute(() =>
            {
                command.GetRequiredService<AnalyzeCommand>().Run(option.ParsedValue);
                return 0;
            });
        }

        public AnalyzeCommand(IMaintenanceService maintenanceService, ILogger<AnalyzeCommand> logger)
        {
            _maintenanceService = maintenanceService;
            _logger = logger;
        }

        public void Run(AnalyzeOption option)
        {
            switch (option)
            {
                case AnalyzeOption.IndexTerms:
                    _maintenanceService.AnalyzeIndexTerms();
                    break;
            }
        }
    }
}
