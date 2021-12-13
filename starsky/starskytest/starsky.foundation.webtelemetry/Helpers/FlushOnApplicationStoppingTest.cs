using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.webtelemetry.Helpers;

namespace starskytest.starsky.foundation.webtelemetry.Helpers
{
	[TestClass]
	public class FlushOnApplicationStoppingTest
	{
		[TestMethod]
		public void ApplicationBuilder_FlushOnApplicationStopping1()
		{
			var services = new ServiceCollection();
			services.AddSingleton<TelemetryClient>();
            
			// build the service
			var serviceProvider = services.BuildServiceProvider();
			
			new FlushApplicationInsights(new ApplicationBuilder(serviceProvider)).Flush();
			var telemetryClient = serviceProvider.GetService<TelemetryClient>();
			Assert.IsNotNull(telemetryClient);
		} 
		
		[TestMethod]
		public async Task ApplicationBuilder_FlushOnApplicationStopping1Async()
		{
			var services = new ServiceCollection();
			services.AddSingleton<TelemetryClient>();
            
			// build the service
			var serviceProvider = services.BuildServiceProvider();
			
			await new FlushApplicationInsights(new ApplicationBuilder(serviceProvider)).FlushAsync();
			var telemetryClient = serviceProvider.GetService<TelemetryClient>();
			Assert.IsNotNull(telemetryClient);
		} 
		
		[TestMethod]
		public void serviceProvider_FlushOnApplicationStopping1()
		{
			var services = new ServiceCollection();
			services.AddSingleton<TelemetryClient>();
            
			// build the service
			var serviceProvider = services.BuildServiceProvider();
			
			new FlushApplicationInsights(serviceProvider).Flush();
			var telemetryClient = serviceProvider.GetService<TelemetryClient>();
			Assert.IsNotNull(telemetryClient);
		} 
		
		[TestMethod]
		public async Task serviceProvider_FlushOnApplicationStopping1Async()
		{
			var services = new ServiceCollection();
			services.AddSingleton<TelemetryClient>();
            
			// build the service
			var serviceProvider = services.BuildServiceProvider();
			
			await new FlushApplicationInsights(serviceProvider).FlushAsync();
			var telemetryClient = serviceProvider.GetService<TelemetryClient>();
			Assert.IsNotNull(telemetryClient);
		} 
	}
}
