using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.Repositories;
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
	
	[TestMethod]
	public void SetupDataProtection_SetsXmlRepositoryFromServiceProvider()
	{
		// Arrange
		var services = new ServiceCollection();
		services.AddSingleton<IXmlRepository, FakeIXmlRepository>();

		// Add the data protection services and configure the key management options
		services.SetupDataProtection();

		var serviceProvider = services.BuildServiceProvider();
		var dataProtectionProvider = serviceProvider.GetService<IDataProtectionProvider>();

		// Act
		var protector = dataProtectionProvider?.CreateProtector("test");

		// Assert
		Assert.IsInstanceOfType(protector, typeof(IDataProtector));
	}

	private class FakeIXmlRepository : IXmlRepository
	{
		public IReadOnlyCollection<XElement> GetAllElements()
		{
			throw new System.NotImplementedException();
		}

		public void StoreElement(XElement element, string friendlyName)
		{
			throw new System.NotImplementedException();
		}
	}
}
