﻿using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using starsky.feature.import.Interfaces;
using starsky.feature.import.Services;
using starsky.foundation.database.Helpers;
using starsky.foundation.http.Interfaces;
using starsky.foundation.injection;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;

namespace starskyimportercli
{
    public static class Program
    {
	    public static async Task Main(string[] args)
        {
            // Use args in application
            new ArgsHelper().SetEnvironmentByArgs(args);

            var services = new ServiceCollection();

            // Setup AppSettings
            services = SetupAppSettings.FirstStepToAddSingleton(services);

            // Inject services
            new RegisterDependencies().Configure(services);
            var serviceProvider = services.BuildServiceProvider();
            var appSettings = serviceProvider.GetRequiredService<AppSettings>();
            
            new SetupDatabaseTypes(appSettings,services).BuilderDb();
            serviceProvider = services.BuildServiceProvider();

            var import = serviceProvider.GetService<IImport>();
            var console = serviceProvider.GetRequiredService<IConsole>();
            var httpClientHelper = serviceProvider.GetRequiredService<IHttpClientHelper>();

            // Help and other Command Line Tools args are included in the ImporterCli 
            await new ImportCli(import, appSettings, console, httpClientHelper).Importer(args);
        }
    }
}
