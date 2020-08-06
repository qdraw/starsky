using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using starsky.feature.webhtmlpublish.Interfaces;
using starsky.foundation.injection;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;

namespace starsky.feature.webhtmlpublish.Services
{
	[Service(typeof(IPublishPreflight), InjectionLifetime = InjectionLifetime.Scoped)]
	public class PublishPreflight : IPublishPreflight
	{
		private readonly AppSettings _appSettings;
		private readonly IConsole _console;

		public PublishPreflight(AppSettings appSettings, IConsole console)
		{
			_appSettings = appSettings;
			_console = console;
		}
		
		public List<Tuple<int,string>> GetPublishProfileNames()
		{
			var returnList = new List<Tuple<int,string>>();
			if ( !_appSettings.PublishProfiles.Any() )
			{
				return returnList;
			}

			var i = 0;
			foreach ( var profile in _appSettings.PublishProfiles )
			{
				returnList.Add(new Tuple<int, string>(i, profile.Key));
				i++;
			}
			return returnList;
		}

		public List<AppSettingsPublishProfiles> GetPublishProfileName(string publishProfileName)
		{
			return _appSettings.PublishProfiles
				.FirstOrDefault(p => p.Key == publishProfileName).Value;
		}

		public string GetPublishProfileNameByIndex(int index)
		{
			return _appSettings.PublishProfiles.ElementAt(index).Key;
		}

		/// <summary>
		/// Get the name by 1. -n or --name argument
		/// Or 2. By user input
		/// or 3. By user input and press enter to use the folder name
		/// </summary>
		/// <param name="inputPath">full filepath to give default user input option</param>
		/// <param name="args">argument list</param>
		/// <returns>name, nothing is string.emthy</returns>
		public string GetNameConsole(string inputPath, IReadOnlyList<string> args)
		{
			var name = new ArgsHelper().GetName(args);
			if ( !string.IsNullOrWhiteSpace(name) ) return name;
			
			var suggestedInput = Path.GetFileName(inputPath);
                
			_console.WriteLine("\nWhat is the name of the item? (for: "+ suggestedInput +" press Enter)\n ");
			name = _console.ReadLine();
			
			if (string.IsNullOrEmpty(name))
			{
				name = suggestedInput;
			}
			return name;
		}
	}

}
