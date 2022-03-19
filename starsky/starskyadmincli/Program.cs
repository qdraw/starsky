using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.accountmanagement.Interfaces;
using starsky.foundation.consoletelemetry.Extensions;
using starsky.foundation.database.Data;
using starsky.foundation.database.Helpers;
using starsky.foundation.injection;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Services;
using starsky.foundation.webtelemetry.Helpers;
using starskyAdminCli.Services;

[assembly: InternalsVisibleTo("starskytest")]
namespace starskyAdminCli
{
	internal static class Program
	{
		/// <summary>
		/// Starsky Admin CLI to manage user admin tasks
		/// </summary>
		/// <param name="args">use -h to see all options</param>
		internal static async Task Main(string[] args)
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
						
			services.AddMonitoringWorkerService(appSettings, AppSettings.StarskyAppType.Geo);
			services.AddApplicationInsightsLogging(appSettings);
			
			var webLogger = serviceProvider.GetRequiredService<IWebLogger>();

			new SetupDatabaseTypes(appSettings,services).BuilderDb();
			serviceProvider = services.BuildServiceProvider();

			// Use args in application
			appSettings.Verbose = ArgsHelper.NeedVerbose(args);
			
			var userManager = serviceProvider.GetService<IUserManager>();
			appSettings.ApplicationType = AppSettings.StarskyAppType.Admin;

			if (new ArgsHelper().NeedHelp(args))
			{
				new ArgsHelper(appSettings).NeedHelpShowDialog();
				return;
			}
			
			await RunMigrations.Run(serviceProvider.GetService<ApplicationDbContext>(), webLogger);
			await new ConsoleAdmin(userManager, new ConsoleWrapper()).Tool(
				ArgsHelper.GetName(args), ArgsHelper.GetUserInputPassword(args));
			
			await new FlushApplicationInsights(serviceProvider).FlushAsync();
		}
	}
}
