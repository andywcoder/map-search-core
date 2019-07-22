using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using Santolibre.Map.Search.CacheUtility.Commands;
using Santolibre.Map.Search.Lib.Repositories;
using Santolibre.Map.Search.Lib.Services;
using System;

namespace Santolibre.Map.Search.CacheUtility
{
    class Program
    {
        public static void Main(string[] args)
        {
            var servicesProvider = SetupServices();

            try
            {
                var app = new CommandLineApplication();
                app.Conventions
                    .SetAppNameFromEntryAssembly()
                    .UseConstructorInjection(servicesProvider);
                RootCommand.Configure(app);
                app.Execute(args);
            }
            catch (Exception e)
            {
                NLog.LogManager.GetCurrentClassLogger().Fatal(e.Message);
            }

            NLog.LogManager.Shutdown();
        }

        private static ServiceProvider SetupServices()
        {
            return new ServiceCollection()
                .AddSingleton<ILocalizationService>(provider => new LocalizationService(null))
                .AddSingleton<ITranslationService, TranslationService>()
                .AddSingleton<IMaintenanceService, MaintenanceService>()
                .AddSingleton<ITranslationRepository, TranslationRepository>()
                .AddSingleton<IDocumentRepository, DocumentRepository>()
                .AddSingleton<IPointOfInterestRepository, PointOfInterestRepository>()
                .AddSingleton<ITranslationCacheRepository, TranslationCacheRepository>()
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
                .AddTransient<ImportCommand>()
                .AddTransient<PurgeCommand>()
                .AddTransient<CompactCommand>()
                .AddTransient<AnalyzeCommand>()
                .AddTransient<UpdateTranslationCacheCommand>()
                .BuildServiceProvider();
        }
    }
}
