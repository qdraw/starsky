using System.Collections.Generic;
using System.Linq;
using starsky.feature.metaupdate.Interfaces;
using starsky.feature.metaupdate.Services;
using starsky.foundation.database.Models;

namespace starskytest.FakeMocks
{
	public class FakeIMetaReplaceService : IMetaReplaceService
	{
		private readonly List<FileIndexItem> _input = new List<FileIndexItem>();

		public FakeIMetaReplaceService(List<FileIndexItem> input = null)
		{
			if ( input != null )
			{
				_input = input;
			}
		}
		
		public List<FileIndexItem> Replace(string f, string fieldName, string search, string replace,
			bool collections)
		{
			return new MetaReplaceService(null, null, null).SearchAndReplace(
				_input.Where(p => p.FilePath == f).ToList(), fieldName, search,
				replace);
		}
	}
}
