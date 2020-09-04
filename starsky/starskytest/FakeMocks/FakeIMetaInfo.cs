using System.Collections.Generic;
using System.Linq;
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
		
		public List<FileIndexItem> GetInfo(List<string> inputFilePaths, bool collections)
		{
			var result = new List<FileIndexItem>();
			foreach ( var path in inputFilePaths )
			{
				var data = Exist.FirstOrDefault(p => p.FilePath == path);
				if ( data == null ) continue;
				result.Add(data);
			}
			return result;
		}
	}
}
