using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Extensions;
using starsky.foundation.platform.Models;

namespace starskytest.starsky.foundation.platform.Middleware
{
	[TestClass]
	public sealed class ServiceCollectionExtensionTest
	{
		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ServiceCollectionExtensions_ServiceCollectionConfigurePoco_null_Config_Test()
		{
			new ServiceCollection().ConfigurePoCo<AppSettings>(null);
		}
		
	}
}
