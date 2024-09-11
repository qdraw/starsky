using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Extensions;
using starsky.foundation.platform.Models;

namespace starskytest.starsky.foundation.platform.Middleware;

[TestClass]
public sealed class ServiceCollectionExtensionTest
{
	[TestMethod]
	public void ServiceCollectionExtensions_ServiceCollectionConfigurePoco_null_Config_Test()
	{
		// Act & Assert
		var exception = Assert.ThrowsException<ArgumentNullException>(() =>
		{
			new ServiceCollection().ConfigurePoCo<AppSettings>(null!);
		});

		// Additional assertions (optional)
		Assert.IsNotNull(exception);
	}
}
