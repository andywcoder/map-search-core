using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using Santolibre.Map.Search.Lib.Repositories;
using Santolibre.Map.Search.Lib.Services;

namespace Santolibre.Map.Search.CacheUtility
{
    class Program
    {
        public static void Main(string[] args)
        {
            var servicesProvider = SetupServices();
            var app = servicesProvider.GetRequiredService<App>();

            app.Run(args);

            NLog.LogManager.Shutdown();
        }

        private static ServiceProvider SetupServices()
        {
            return new ServiceCollection()
                .AddSingleton<ILocalizationService>(provider => new LocalizationService(null))
                .AddSingleton<IMaintenanceService, MaintenanceService>()
                .AddSingleton<ISearchService, SearchService>()
                .AddSingleton<ILocationSearchService, MapQuestSearchService>()
                .AddSingleton<IDocumentService, DocumentService>()
                .AddSingleton<IPointOfInterestRepository, PointOfInterestRepository>()
                .AddSingleton<IConfiguration>(new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).Build())
                .AddLogging(builder =>
                {
                    builder.SetMinimumLevel(LogLevel.Trace);
                    builder.AddNLog(new NLogProviderOptions
                    {
                        CaptureMessageTemplates = true,
                        CaptureMessageProperties = true
                    });
                })
                .AddTransient<App>()
                .BuildServiceProvider();
        }
    }
}
