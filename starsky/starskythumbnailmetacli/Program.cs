using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.database.Helpers;
using starsky.foundation.injection;
using starsky.foundation.metathumbnail.Helpers;
using starsky.foundation.metathumbnail.Interfaces;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;

namespace starskythumbnailmetacli
{
	public static class Program
	{
		public static async Task Main(string[] args)
		{
			// Use args in application
			new ArgsHelper().SetEnvironmentByArgs(args);

			var services = new ServiceCollection();

			// Setup AppSettings
			services = await SetupAppSettings.FirstStepToAddSingleton(services);

			// Inject services
			RegisterDependencies.Configure(services);
			var serviceProvider = services.BuildServiceProvider();
			var appSettings = serviceProvider.GetRequiredService<AppSettings>();
            
			serviceProvider = services.BuildServiceProvider();
			var selectorStorage = serviceProvider.GetRequiredService<ISelectorStorage>();

			var console = serviceProvider.GetRequiredService<IConsole>();
			var metaExifThumbnailService = serviceProvider.GetRequiredService<IMetaExifThumbnailService>();

			// Help and other Command Line Tools args are included in the Geo tools 
			await new MetaThumbnailCommandLineHelper(selectorStorage, 
				appSettings, console, metaExifThumbnailService ).CommandLineAsync(args);
		}
	}
}
