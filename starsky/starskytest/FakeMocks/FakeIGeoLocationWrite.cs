using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.foundation.database.Models;
using starsky.foundation.writemeta.Interfaces;

namespace starskytest.FakeMocks
{
	public class FakeIGeoLocationWrite : IGeoLocationWrite
	{
		// ReSharper disable once MemberCanBePrivate.Global
		public List<List<FileIndexItem>> Inputs { get; set; } = new List<List<FileIndexItem>>();
		
		public Task LoopFolderAsync(List<FileIndexItem> metaFilesInDirectory,
			bool syncLocationNames)
		{
			Inputs.Add(metaFilesInDirectory);
			return Task.CompletedTask;
		}
	}
}
