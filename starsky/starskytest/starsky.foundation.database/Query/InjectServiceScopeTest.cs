using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Query;

namespace starskytest.starsky.foundation.database.Query
{
	[TestClass]
	public class InjectServiceScopeTest
	{
		[TestMethod]
		public void NoScope()
		{
			new InjectServiceScope(null).Context();
		}
	}
}
