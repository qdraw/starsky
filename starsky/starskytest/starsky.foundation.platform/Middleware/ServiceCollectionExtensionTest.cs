using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Middleware;
using starsky.foundation.platform.Models;

namespace starskytest.Middleware
{
	[TestClass]
	public class ServiceCollectionExtensionTest
	{
		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ServiceCollectionExtensions_ServiceCollectionConfigurePoco_null_Config_Test()
		{
			new ServiceCollection().ConfigurePoCo<AppSettings>(null);
		}
		
		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ServiceCollectionExtensions_ServiceCollectionConfigurePoco_nullTest()
		{
			var serviceCollection = new ServiceCollection();
			serviceCollection = null;
			serviceCollection.ConfigurePoCo<AppSettings>(null);
		}
		
	}
}
