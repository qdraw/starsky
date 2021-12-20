using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.consoletelemetry.Extensions;
using starsky.foundation.platform.Models;

namespace starskytest.starsky.foundation.consoletelemetry.Extensions
{
	[TestClass]
	public class ApplicationInsightsWorkerExtensionTest
	{
		[TestMethod]
		public void TestIfServiceIsEnabled()
		{
			var serviceCollection = new ServiceCollection();
			IConfiguration configuration = new ConfigurationRoot(new List<IConfigurationProvider>());
			serviceCollection.AddSingleton(configuration); 
			
			serviceCollection.AddMonitoringWorkerService(new AppSettings{ApplicationInsightsInstrumentationKey = "t"}, AppSettings.StarskyAppType.Importer);

			Assert.IsTrue(serviceCollection.Count >= 1);
			var result= serviceCollection.FirstOrDefault(p
				=> p.ServiceType.FullName.Contains("ApplicationInsights"));
			Assert.IsNotNull(result);
		}
		
		[TestMethod]
		public void TestIfServiceIsDisabled()
		{
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddMonitoringWorkerService(new AppSettings{ApplicationInsightsInstrumentationKey = ""}, AppSettings.StarskyAppType.Importer);

			Assert.AreEqual(0, serviceCollection.Count());
			var result= serviceCollection.FirstOrDefault(p
				=> p.ServiceType.FullName.Contains("ApplicationInsights"));
			Assert.IsNull(result);
		}
	}
}
