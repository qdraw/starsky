using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.DataProtection;

namespace starskytest.starsky.foundation.database.DataProtection;

[TestClass]
public class AddDataProtectionKeysTest
{
	[TestMethod]
	public void AddDataProtectionKeys()
	{
		var service = new ServiceCollection() as IServiceCollection;

		service.SetupDataProtection();
		
		Assert.IsTrue(service.Any(p => p.ToString().Contains("DataProtection")));
	}
}
