using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using starsky.feature.import.Interfaces;
using starsky.feature.import.Services;
using starsky.foundation.database.Helpers;
using starsky.foundation.database.Import;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Query;
using starsky.foundation.http.Interfaces;
using starsky.foundation.injection;
using starsky.foundation.platform.Extensions;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.writemeta.Interfaces;

namespace starskyImporterNetFrameworkCli
{
	static class Program
	{
		static async Task Main(string[] args)
		{
			// Use args in application
			new ArgsHelper().SetEnvironmentByArgs(args);

			var services = new ServiceCollection();

			// Setup AppSettings
			services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
			var configurationRoot = await SetupAppSettings.AppSettingsToBuilder();
			services.ConfigurePoCo<AppSettings>(configurationRoot.GetSection("App"));

			// Inject services
			new RegisterDependencies().Configure(services);
			var serviceProvider = services.BuildServiceProvider();
			var appSettings = serviceProvider.GetRequiredService<AppSettings>();
            
			appSettings.Verbose = new ArgsHelper().NeedVerbose(args);

			new SetupDatabaseTypes(appSettings,services).BuilderDb();
			
			// remove ImportQuery and use legacy class
			var serviceDescriptorImportQuery = services.FirstOrDefault(descriptor => descriptor.ServiceType == typeof(IImportQuery));
			services.Remove(serviceDescriptorImportQuery);
			services.AddScoped<IImportQuery,ImportQueryNetFramework>();
			
			// remove Query and use legacy class
			var serviceDescriptorQuery = services.FirstOrDefault(descriptor => descriptor.ServiceType == typeof(IQuery));
			services.Remove(serviceDescriptorQuery);
			services.AddScoped<IQuery,QueryNetFramework>();
			
			serviceProvider = services.BuildServiceProvider();

			var import = serviceProvider.GetService<IImport>();
			var console = serviceProvider.GetRequiredService<IConsole>();
			var exifToolDownload = serviceProvider.GetRequiredService<IExifToolDownload>();

			await new ImportCli(import, appSettings, console, exifToolDownload).Importer(args);
		}
	}
}
