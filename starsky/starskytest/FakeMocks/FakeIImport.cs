using System.Collections.Generic;
using starskycore.Interfaces;
using starskycore.Models;

namespace starskytest.FakeMocks
{
	public class FakeIImport : IImport
	{
		public List<string> Import(IEnumerable<string> inputFullPathList, ImportSettingsModel importSettings)
		{
			throw new System.NotImplementedException();
		}

		public List<ImportIndexItem> Preflight(List<string> inputFileFullPaths, ImportSettingsModel importSettings)
		{
			throw new System.NotImplementedException();
		}

		public List<ImportIndexItem> History()
		{
			throw new System.NotImplementedException();
		}

	}
}
