using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.feature.metaupdate.Interfaces;
using starsky.foundation.database.Models;

namespace starskytest.FakeMocks
{
	public class FakeIMetaInfo : IMetaInfo
	{
		private List<FileIndexItem> Exist { get; set; }

		public FakeIMetaInfo(List<FileIndexItem> existItems)
		{
			Exist = existItems;
			if ( Exist == null )
			{
				Exist = new List<FileIndexItem>();
			}
		}

		public Task<List<FileIndexItem>> GetInfoAsync(List<string> inputFilePaths, bool collections)
		{
			var result = new List<FileIndexItem>();
			foreach ( var path in inputFilePaths )
			{
				var data = Exist.Find(p => p.FilePath == path);
				if ( data == null ) continue;
				result.Add(data);
			}

			return Task.FromResult(result);
		}
	}
}
