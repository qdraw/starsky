using System.Collections.Generic;
using starsky.foundation.database.Models;
using starsky.foundation.writemeta.Interfaces;

namespace starskytest.FakeMocks
{
	public class FakeIGeoLocationWrite : IGeoLocationWrite
	{
		public List<List<FileIndexItem>> Inputs { get; set; } = new List<List<FileIndexItem>>();
		public void LoopFolder(List<FileIndexItem> metaFilesInDirectory, bool syncLocationNames)
		{
			Inputs.Add(metaFilesInDirectory);
		}
	}
}
