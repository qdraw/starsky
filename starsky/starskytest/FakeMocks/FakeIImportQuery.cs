using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;

namespace starskytest.FakeMocks
{
	public class FakeIImportQuery : IImportQuery
	{
		private readonly List<string> _exist;
		private readonly bool _isConnection;

		public FakeIImportQuery(List<string> exist, bool isConnection = true)
		{
			_exist = exist;
			_isConnection = isConnection;
			if ( exist == null ) _exist = new List<string>();
		}
		
		public async Task<bool> IsHashInImportDbAsync(string fileHashCode)
		{
			return _exist.Contains(fileHashCode);
		}

		public bool TestConnection()
		{
			return _isConnection;
		}

		public async Task<bool> AddAsync(ImportIndexItem updateStatusContent)
		{
			_exist.Add(updateStatusContent.FileHash);
			return true;
		}
	}
}
