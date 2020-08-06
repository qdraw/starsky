using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using starsky.feature.webhtmlpublish.Helpers;
using starsky.feature.webhtmlpublish.Interfaces;
using starsky.foundation.injection;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Middleware;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;

namespace starskywebhtmlcli
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
	        // Use args in application
	        new ArgsHelper().SetEnvironmentByArgs(args);
	        var services = new ServiceCollection();

	        // Setup AppSettings
	        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
	        var configurationRoot = SetupAppSettings.AppSettingsToBuilder();
	        services.ConfigurePoCo<AppSettings>(configurationRoot.GetSection("App"));

	        // Inject services
	        new RegisterDependencies().Configure(services);
	        var serviceProvider = services.BuildServiceProvider();
	        var appSettings = serviceProvider.GetRequiredService<AppSettings>();
            
	        appSettings.Verbose = new ArgsHelper().NeedVerbose(args);

	        // dont need database services?: new SetupDatabaseTypes(appSettings,services).BuilderDb();
	        serviceProvider = services.BuildServiceProvider();

	        var publishPreflight = serviceProvider.GetService<IPublishPreflight>();
	        var publishService = serviceProvider.GetService<IWebHtmlPublishService>();
	        var storageSelector = serviceProvider.GetService<ISelectorStorage>();

	        var console = serviceProvider.GetRequiredService<IConsole>();

	        // Help and args selectors are defined in the PublishCli
	        await new PublishCli(storageSelector, publishPreflight, publishService, appSettings, console).Publisher(args);
		}
        
    }
}
