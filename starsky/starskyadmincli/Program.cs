using System;
using System.Runtime.CompilerServices;
using starskyAdminCli.Models;
using starskyAdminCli.Services;
using starskycore.Helpers;
using starskycore.Models;
using starskycore.Services;

[assembly: InternalsVisibleTo("starskytest")]
namespace starskyAdminCli
{
	internal class Program
	{
		internal static void Main(string[] args)
		{
			new ArgsHelper().SetEnvironmentByArgs(args);

			var startupHelper = new ConfigCliAppsStartupHelper();
			var appSettings = startupHelper.AppSettings();

			// Use args in application
			appSettings.Verbose = new ArgsHelper().NeedVerbose(args);

			new ConsoleAdmin(appSettings, startupHelper.UserManager(), new ConsoleWrapper());
		}
		
	}
}
