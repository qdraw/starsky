using System.Collections.Generic;
using starsky.feature.metaupdate.Interfaces;
using starsky.foundation.database.Models;

namespace starskytest.FakeMocks
{
	public class FakeIMetaInfo : IMetaInfo
	{
		public List<FileIndexItem> GetInfo(List<string> inputFilePaths, bool collections)
		{
			return new List<FileIndexItem>();
		}
	}
}
