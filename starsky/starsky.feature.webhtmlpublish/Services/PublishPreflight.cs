using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using starsky.feature.webhtmlpublish.Helpers;
using starsky.feature.webhtmlpublish.Interfaces;
using starsky.foundation.injection;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;

namespace starsky.feature.webhtmlpublish.Services
{
	[Service(typeof(IPublishPreflight), InjectionLifetime = InjectionLifetime.Scoped)]
	public class PublishPreflight : IPublishPreflight
	{
		private readonly AppSettings _appSettings;
		private readonly IConsole _console;
		private readonly IWebLogger _logger;
		private readonly IStorage _hostStorage;

		public PublishPreflight(AppSettings appSettings, IConsole console,
			ISelectorStorage selectorStorage, IWebLogger logger)
		{
			_appSettings = appSettings;
			_console = console;
			_logger = logger;
			_hostStorage = selectorStorage.Get(SelectorStorage.StorageServices.HostFilesystem);
		}

		public List<Tuple<int, string>> GetPublishProfileNames()
		{
			var returnList = new List<Tuple<int, string>>();
			if ( _appSettings.PublishProfiles == null || _appSettings.PublishProfiles.Count == 0 )
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

		/// <summary>
		/// Check if the profile is valid
		/// </summary>
		/// <param name="publishProfileName">profile key</param>
		/// <returns>(bool and list of errors)</returns>
		public Tuple<bool, List<string>> IsProfileValid(
			string publishProfileName)
		{
			var profiles = _appSettings.PublishProfiles!
				.FirstOrDefault(p => p.Key == publishProfileName);
			return IsProfileValid(profiles);
		}

		/// <summary>
		/// Check if the profile is valid
		/// </summary>
		/// <param name="profiles">profile object</param>
		/// <returns>(bool and list of errors)</returns>
		internal Tuple<bool, List<string>> IsProfileValid(
			KeyValuePair<string, List<AppSettingsPublishProfiles>> profiles)
		{
			if ( profiles.Key == null || profiles.Value == null )
			{
				return new Tuple<bool, List<string>>(false,
					new List<string> { "Profile not found" });
			}

			var errors = new List<string>();
			foreach ( var profile in profiles.Value )
			{
				if ( string.IsNullOrEmpty(profile.Path) )
				{
					continue;
				}

				if ( profile.ContentType == TemplateContentType.Html
					 && !new ParseRazor(_hostStorage, _logger).Exist(profile.Template) )
				{
					errors.Add($"View Path {profile.Template} should exists");
					continue;
				}

				if ( !_hostStorage.ExistFile(profile.Path) && (
						profile.ContentType == TemplateContentType.Jpeg
						|| profile.ContentType == TemplateContentType.OnlyFirstJpeg ) )
				{
					errors.Add($"Image Path {profile.Path} should exists");
				}
			}

			return new Tuple<bool, List<string>>(errors.Count == 0, errors);
		}

		/// <summary>
		/// Get all publish profile names
		/// </summary>
		/// <returns>(string: name, bool: isValid)</returns>
		public IEnumerable<KeyValuePair<string, bool>> GetAllPublishProfileNames()
		{
			return _appSettings.PublishProfiles!.Select(p =>
				new KeyValuePair<string, bool>(
					p.Key, IsProfileValid(p).Item1));
		}

		public List<AppSettingsPublishProfiles> GetPublishProfileName(string publishProfileName)
		{
			return _appSettings.PublishProfiles!
				.FirstOrDefault(p => p.Key == publishProfileName).Value;
		}

		public string GetPublishProfileNameByIndex(int index)
		{
			return _appSettings.PublishProfiles!.ElementAtOrDefault(index).Key;
		}

		/// <summary>
		/// Get the name by 1. -n or --name argument
		/// Or 2. By user input
		/// or 3. By user input and press enter to use the folder name
		/// </summary>
		/// <param name="inputPath">full filepath to give default user input option</param>
		/// <param name="args">argument list</param>
		/// <returns>name, nothing is string.empty</returns>
		public string GetNameConsole(string inputPath, IReadOnlyList<string> args)
		{
			var name = ArgsHelper.GetName(args);
			if ( !string.IsNullOrWhiteSpace(name) )
			{
				return name;
			}

			var suggestedInput = Path.GetFileName(inputPath);

			_console.WriteLine("\nWhat is the name of the item? (for: " + suggestedInput +
							   " press Enter)\n ");
			name = _console.ReadLine();

			if ( string.IsNullOrEmpty(name) )
			{
				name = suggestedInput;
			}

			return name.Trim();
		}
	}
}
