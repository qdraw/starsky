using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.accountmanagement.Interfaces;
using starsky.foundation.database.Data;
using starsky.foundation.database.Helpers;
using starsky.foundation.injection;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Services;
using starskyAdminCli.Services;

[assembly: InternalsVisibleTo("starskytest")]
namespace starskyAdminCli
{
	internal static class Program
	{
		internal static async Task Main(string[] args)
		{
			// Use args in application
			new ArgsHelper().SetEnvironmentByArgs(args);

			var services = new ServiceCollection();

			// Setup AppSettings
			services = await SetupAppSettings.FirstStepToAddSingleton(services);

			// Inject services
			new RegisterDependencies().Configure(services);
			var serviceProvider = services.BuildServiceProvider();
			var appSettings = serviceProvider.GetRequiredService<AppSettings>();
            
			new SetupDatabaseTypes(appSettings,services).BuilderDb();
			serviceProvider = services.BuildServiceProvider();

			// Use args in application
			appSettings.Verbose = new ArgsHelper().NeedVerbose(args);
			
			var userManager = serviceProvider.GetService<IUserManager>();
			appSettings.ApplicationType = AppSettings.StarskyAppType.Admin;

			if (new ArgsHelper().NeedHelp(args))
			{
				new ArgsHelper(appSettings).NeedHelpShowDialog();
				return;
			}
			
			await RunMigrations.Run(serviceProvider.GetService<ApplicationDbContext>());
			new ConsoleAdmin(userManager, new ConsoleWrapper()).Tool(
				new ArgsHelper().GetName(args), new ArgsHelper().GetUserInputPassword(args));
		}
	}
}
