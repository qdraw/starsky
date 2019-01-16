using System;
using System.Collections.Generic;
using System.Text;
using starskycore.Interfaces;

namespace starskytests.FakeMocks
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
