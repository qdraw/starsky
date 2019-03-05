using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starskycore.Middleware;
using starskycore.Models;

namespace starskytest.Middleware
{
	[TestClass]
	public class ServiceCollectionExtensionTest
	{
		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ServiceCollectionExtensions_ServiceCollectionConfigurePoco_null_Config_Test()
		{
			new ServiceCollection().ConfigurePoco<AppSettings>(null);
		}
		
		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ServiceCollectionExtensions_ServiceCollectionConfigurePoco_nullTest()
		{
			var serviceCollection = new ServiceCollection();
			serviceCollection = null;
			serviceCollection.ConfigurePoco<AppSettings>(null);
		}
		
	}
}
