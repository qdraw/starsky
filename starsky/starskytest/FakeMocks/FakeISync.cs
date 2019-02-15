using System.Collections.Generic;
using starskycore.Interfaces;

namespace starskytest.FakeMocks
{
	class FakeISync : ISync
	{
		public IEnumerable<string> SyncFiles(string subPath, bool recursive = true)
		{
			return new List<string>{ subPath };
		}

		public void AddSubPathFolder(string subPath)
		{
			
		}
	}
}
