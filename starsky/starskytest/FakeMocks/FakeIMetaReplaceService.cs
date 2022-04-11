using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using starsky.feature.metaupdate.Interfaces;
using starsky.feature.metaupdate.Services;
using starsky.foundation.database.Models;

namespace starskytest.FakeMocks
{
	public class FakeIMetaReplaceServiceData
	{
		public string f { get; set; }
		public string fieldName { get; set; }
		public string search { get; set; }
		public string replace { get; set; }
	}
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

		public List<FakeIMetaReplaceServiceData> Data { get; set; } = new List<FakeIMetaReplaceServiceData>();
		
		public Task<List<FileIndexItem>> Replace(string f, string fieldName, string search, string replace,
			bool collections)
		{
			Data.Add(new FakeIMetaReplaceServiceData{f = f,fieldName = fieldName, search = search, replace = replace});
			return Task.FromResult(MetaReplaceService.SearchAndReplace(
				_input.Where(p => p.FilePath == f).ToList(), fieldName, search,
				replace));
		}
	}
}
