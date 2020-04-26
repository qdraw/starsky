using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;

namespace starskytest.FakeMocks
{
	public class FakeIImportQuery : IImportQuery
	{
		private readonly List<string> _exist;

		public FakeIImportQuery(List<string> exist)
		{
			_exist = exist;
			if ( exist == null ) _exist = new List<string>();
		}
		
		public async Task<bool> IsHashInImportDbAsync(string fileHashCode)
		{
			return _exist.Contains(fileHashCode);
		}

		public bool TestConnection()
		{
			throw new System.NotImplementedException();
		}

		public Task<bool> AddAsync(ImportIndexItem updateStatusContent)
		{
			throw new System.NotImplementedException();
		}
	}
}
