using System;
using System.Linq;
using starsky.feature.webhtmlpublish.Helpers;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Helpers;
using starsky.foundation.storage.Models;
using starsky.foundation.storage.Storage;
using starskycore.Helpers;
using starskywebftpcli.Services;

namespace starskywebftpcli
{
	static class Program
	{
		static void Main(string[] args)
		{
			throw new Exception("remove starskyCore reference");
			
			// Use args in application
			new ArgsHelper().SetEnvironmentByArgs(args);
			
			// inject services + appSettings
			var startupHelper = new ConfigCliAppsStartupHelper();
			var appSettings = startupHelper.AppSettings();
            
			
			// verbose mode
			appSettings.Verbose = new ArgsHelper().NeedVerbose(args);
            
			
			

			
			// TODO: Replace TEst with actual name
			
			// // get prepend path to show
			// var prepend = appSettings.GetWebSafeReplacedName(
			// 	appSettings.PublishProfiles.FirstOrDefault(p => p.Key == "test").Value
			// 		.FirstOrDefault(p => !string.IsNullOrEmpty(p.Prepend))
			// 		?.Prepend
			// );
			
			// show prepend path!
			// Console.WriteLine(prepend);

		}
	}
}
