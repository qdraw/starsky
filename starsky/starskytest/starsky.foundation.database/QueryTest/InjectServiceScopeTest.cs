using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Data;
using starsky.foundation.database.Query;

namespace starskytest.starsky.foundation.database.QueryTest
{
	[TestClass]
	public sealed class InjectServiceScopeTest
	{
		[TestMethod]
		public void NoScope()
		{
			IServiceScopeFactory? scope = null;
			new InjectServiceScope(scope).Context();
			Assert.IsNull(scope);
		}
	}
}
