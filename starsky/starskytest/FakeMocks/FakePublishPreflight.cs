using System.Collections.Generic;
using starsky.feature.webhtmlpublish.Interfaces;
using starsky.foundation.platform.Models;

namespace starskytest.FakeMocks
{
	public class FakeIPublishPreflight : IPublishPreflight
	{
		public IEnumerable<string> GetAllPublishProfileNames()
		{
			throw new System.NotImplementedException();
		}

		public string GetNameConsole(string inputPath, IReadOnlyList<string> args)
		{
			throw new System.NotImplementedException();
		}

		public List<AppSettingsPublishProfiles> GetPublishProfileName(string publishProfileName)
		{
			throw new System.NotImplementedException();
		}
	}
}
