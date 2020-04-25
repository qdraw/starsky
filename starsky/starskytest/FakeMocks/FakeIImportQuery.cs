using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.foundation.database.Interfaces;

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
		
		public async Task<bool> IsHashInImportDb(string fileHashCode)
		{
			return _exist.Contains(fileHashCode);
		}
	}
}
