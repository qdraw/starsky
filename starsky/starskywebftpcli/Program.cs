using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using starsky.feature.webftppublish.FtpAbstractions.Interfaces;
using starsky.feature.webftppublish.Helpers;
using starsky.foundation.injection;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;

namespace starskywebftpcli
{
	public static class Program
	{
		public static async Task Main(string[] args)
		{
			// Use args in application
			new ArgsHelper().SetEnvironmentByArgs(args);

			// Setup AppSettings
			var services = await SetupAppSettings.FirstStepToAddSingleton(new ServiceCollection());

			// Inject services
			RegisterDependencies.Configure(services);
			var serviceProvider = services.BuildServiceProvider();
			var appSettings = serviceProvider.GetRequiredService<AppSettings>();

			serviceProvider = services.BuildServiceProvider();

			var storageSelector = serviceProvider.GetRequiredService<ISelectorStorage>();
			var console = serviceProvider.GetRequiredService<IConsole>();
			var webRequestFactory = serviceProvider.GetRequiredService<IFtpWebRequestFactory>();

			await new WebFtpCli(appSettings, storageSelector, console, webRequestFactory)
				.RunAsync(args);
		}
	}
}
