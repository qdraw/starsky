using System;
using System.Collections.Generic;
using starsky.feature.webhtmlpublish.Interfaces;
using starsky.foundation.platform.Models;

namespace starskytest.FakeMocks
{
	public class FakeIPublishPreflight : IPublishPreflight
	{
		public IEnumerable<string> GetAllPublishProfileNames()
		{
			Console.WriteLine("GetAllPublishProfileNames -> mocking data with test");
			return new List<string>{"test"};
		}

		public string GetNameConsole(string inputPath, IReadOnlyList<string> args)
		{
			Console.WriteLine("GetNameConsole = always test");
			return "test";
		}

		public List<AppSettingsPublishProfiles> GetPublishProfileName(string publishProfileName)
		{
			throw new System.NotImplementedException();
		}
	}
}
