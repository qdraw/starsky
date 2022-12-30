using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Models;
using starsky.foundation.webtelemetry.Helpers;
using starskytest.FakeMocks;
using starskytest.starsky.foundation.platform.Services;

namespace starskytest.starsky.foundation.webtelemetry.Helpers
{
	[TestClass]
	public sealed class FlushOnApplicationStoppingTest
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


		[TestMethod]
		public void GetTelemetryClientTestKeyShouldHitLogger()
		{
			var logger = new FakeIWebLogger();
			new FlushApplicationInsights(new ServiceCollection()
				.BuildServiceProvider(), new AppSettings{ApplicationInsightsConnectionString = "t"}, logger).GetTelemetryClient();
			Assert.AreEqual(1, logger.TrackedInformation.Count);
			Assert.AreEqual("TelemetryClient is null on exit", logger.TrackedInformation[0].Item2);
		}
		
		[TestMethod]
		public void GetTelemetryClientNullable()
		{
			var logger = new FakeIWebLogger();
			new FlushApplicationInsights(null!, new AppSettings{ApplicationInsightsConnectionString = "t"}, logger).GetTelemetryClient();
			Assert.AreEqual(1, logger.TrackedInformation.Count);
			Assert.AreEqual("TelemetryClient is null on exit", logger.TrackedInformation[0].Item2);
		}
		
		[TestMethod]
		public void GetTelemetryClient_Logger_Nullable()
		{
			var result = new FlushApplicationInsights(null!, new AppSettings{ApplicationInsightsConnectionString = "t"}).GetTelemetryClient();
			Assert.AreEqual(null, result);
		}
		
		[TestMethod]
		public void FlushApplicationInsights_HitLogger_Sync()
		{
			var logger = new FakeIWebLogger();
			new FlushApplicationInsights(new ServiceCollection()
				.BuildServiceProvider(), new AppSettings{ApplicationInsightsConnectionString = "t"}, logger).Flush();
			Assert.AreEqual(1, logger.TrackedInformation.Count);
			Assert.AreEqual("TelemetryClient is null on exit", logger.TrackedInformation[0].Item2);
		}
		
		[TestMethod]
		public async Task FlushApplicationInsights_HitLogger_aSync()
		{
			var logger = new FakeIWebLogger();
			await new FlushApplicationInsights(new ServiceCollection()
				.BuildServiceProvider(), new AppSettings{ApplicationInsightsConnectionString = "t"}, logger).FlushAsync();
			Assert.AreEqual(1, logger.TrackedInformation.Count);
			Assert.AreEqual("TelemetryClient is null on exit", logger.TrackedInformation[0].Item2);
		}

		
		[TestMethod]
		public void GetTelemetryClientTestIgnore()
		{
			var logger = new FakeIWebLogger();
			new FlushApplicationInsights(new ServiceCollection()
				.BuildServiceProvider(), new AppSettings{ApplicationInsightsConnectionString = ""}, logger).GetTelemetryClient();
			Assert.AreEqual(0, logger.TrackedInformation.Count);
		}
	}
}
