using System;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.webtelemetry.Helpers;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.webtelemetry.Helpers
{
	[TestClass]
	public class FlushOnApplicationStoppingTest
	{
		[TestMethod]
		public void FlushOnApplicationStopping1()
		{
			var services = new ServiceCollection();
			services.AddSingleton<TelemetryClient>();
            
			// build the service
			var serviceProvider = services.BuildServiceProvider();
			
			new FlushOnApplicationStopping(new ApplicationBuilder(serviceProvider)).Flush();
			var telemetryClient = serviceProvider.GetService<TelemetryClient>();
			Assert.IsNotNull(telemetryClient);
		} 
	}
}
