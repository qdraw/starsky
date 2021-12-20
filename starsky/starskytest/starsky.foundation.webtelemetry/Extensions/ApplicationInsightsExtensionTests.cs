using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.ApplicationInsights.Extensibility.EventCounterCollector;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Models;
using starsky.foundation.webtelemetry.Extensions;

namespace starskytest.starsky.foundation.webtelemetry.Extensions
{
	[TestClass]
	public class ApplicationInsightsExtensionTests
	{
		[TestMethod]
		public void TestIfServiceIsEnabled()
		{
			var serviceCollection = new ServiceCollection();
			IConfiguration configuration = new ConfigurationRoot(new List<IConfigurationProvider>());
			serviceCollection.AddSingleton(configuration); 
			
			serviceCollection.AddMonitoring(new AppSettings{ApplicationInsightsInstrumentationKey = "t"});

			Assert.IsTrue(serviceCollection.Count >= 1);
			var result= serviceCollection.FirstOrDefault(p
				=> p.ServiceType.FullName.Contains("ApplicationInsights"));
			Assert.IsNotNull(result);
		}
		
		[TestMethod]
		public void TestIfServiceIsDisabled()
		{
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddMonitoring(new AppSettings{ApplicationInsightsInstrumentationKey = ""});

			Assert.AreEqual(0, serviceCollection.Count());
			var result= serviceCollection.FirstOrDefault(p
				=> p.ServiceType.FullName.Contains("ApplicationInsights"));
			Assert.IsNull(result);
		}

		[TestMethod]
		public void SetEventCounterCollectionModule_ShouldContainItems()
		{
			var module = new EventCounterCollectionModule();
			ApplicationInsightsExtension.SetEventCounterCollectionModule(module);
			Assert.IsTrue(module.Counters.Count >= 1);
		}
	}
}
