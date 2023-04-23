using System;
using System.Collections.Generic;
using starsky.feature.webhtmlpublish.Interfaces;
using starsky.foundation.platform.Models;

namespace starskytest.FakeMocks
{
	public class FakeIPublishPreflight : IPublishPreflight
	{
		private readonly List<AppSettingsPublishProfiles> _input = new List<AppSettingsPublishProfiles>();
		private readonly bool _isOk;

		public FakeIPublishPreflight(List<AppSettingsPublishProfiles> input = null, bool isOk = true)
		{
			if ( input != null )
			{
				_input = input;
			}

			_isOk = isOk;
		}
		
		public IEnumerable<KeyValuePair<string,bool>> GetAllPublishProfileNames()
		{
			Console.WriteLine("GetAllPublishProfileNames -> mocking data with test");
			return new List<KeyValuePair<string,bool>>{new KeyValuePair<string, bool>("test",_isOk)};
		}

		public Tuple<bool, List<string>> IsProfileValid(string publishProfileName)
		{
			return new Tuple<bool, List<string>>(_isOk, new List<string>());
		}

		public string GetNameConsole(string inputPath, IReadOnlyList<string> args)
		{
			Console.WriteLine("GetNameConsole = always test");
			return "test";
		}

		public List<AppSettingsPublishProfiles> GetPublishProfileName(string publishProfileName)
		{
			return _input;
		}
	}
}
