using System.Runtime.CompilerServices;
using starskyAdminCli.Services;
using starskycore.Helpers;
using starskycore.Models;
using starskycore.Services;

[assembly: InternalsVisibleTo("starskytest")]
namespace starskyAdminCli
{
	internal static class Program
	{
		internal static void Main(string[] args)
		{
			new ArgsHelper().SetEnvironmentByArgs(args);

			var startupHelper = new ConfigCliAppsStartupHelper();
			var appSettings = startupHelper.AppSettings();
			
			// Use args in application
			appSettings.Verbose = new ArgsHelper().NeedVerbose(args);
			
			if (new ArgsHelper().NeedHelp(args))
			{
				appSettings.ApplicationType = AppSettings.StarskyAppType.Admin;
				new ArgsHelper(appSettings).NeedHelpShowDialog();
				return;
			}
			new ConsoleAdmin(appSettings, startupHelper.UserManager(), new ConsoleWrapper()).Tool();
		}
	}
}
