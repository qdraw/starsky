using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using starsky.foundation.platform.Models;
using starsky.foundation.webtelemetry.Extensions;

namespace starskytest.starsky.foundation.webtelemetry.Helpers;

[TestClass]
public class OpenTelemetryExtensionTest
{

	[TestMethod]
	public void ConfiguresTelemetryBuilder()
	{
		// Arrange
		var services = new ServiceCollection();
		var appSettings = new AppSettings
		{
			OpenTelemetry = new OpenTelemetrySettings
			{
				// Set OpenTelemetry settings as needed for the test
				TracesEndpoint = "https://example.com/traces",
				MetricsEndpoint = "https://example.com/metrics"
			}
		};

		// Act
		services.AddOpenTelemetryMonitoring(appSettings);

		// Assert
		var serviceProvider = services.BuildServiceProvider();


		// Verify tracing configuration
		var tracerProvider = serviceProvider.GetRequiredService<TracerProvider>();
		Assert.IsNotNull(tracerProvider);

		// Verify metrics configuration
		var meterProvider = serviceProvider.GetRequiredService<MeterProvider>();
		Assert.IsNotNull(meterProvider);
	}
	
	[TestMethod]
	public void ConfiguresTelemetryBuilder_Skip_WhenTraces()
	{
		// Arrange
		var services = new ServiceCollection();
		var appSettings = new AppSettings
		{
			OpenTelemetry = new OpenTelemetrySettings
			{
				// Set OpenTelemetry settings as needed for the test
				TracesEndpoint = null,
				MetricsEndpoint = "https://example.com/metrics"
			}
		};

		// Act
		services.AddOpenTelemetryMonitoring(appSettings);

		// Assert
		var serviceProvider = services.BuildServiceProvider();


		// Verify tracing configuration
		var tracerProvider = serviceProvider.GetService<TracerProvider>();
		Assert.IsNull(tracerProvider);

		// Verify metrics configuration
		var meterProvider = serviceProvider.GetRequiredService<MeterProvider>();
		Assert.IsNotNull(meterProvider);
	}
	
	[TestMethod]
	public void ConfiguresTelemetryBuilder_Skip_WhenMetrics()
	{
		// Arrange
		var services = new ServiceCollection();
		var appSettings = new AppSettings
		{
			OpenTelemetry = new OpenTelemetrySettings
			{
				// Set OpenTelemetry settings as needed for the test
				TracesEndpoint = "https://example.com/traces",
				MetricsEndpoint = null
			}
		};

		// Act
		services.AddOpenTelemetryMonitoring(appSettings);

		// Assert
		var serviceProvider = services.BuildServiceProvider();


		// Verify tracing configuration
		var tracerProvider = serviceProvider.GetService<TracerProvider>();
		Assert.IsNotNull(tracerProvider);

		// Verify metrics configuration
		var meterProvider = serviceProvider.GetService<MeterProvider>();
		Assert.IsNull(meterProvider);
	}

	[TestMethod]
	public void FilterPathTestIsTrue()
	{
		var context = new DefaultHttpContext { Request = { Path = "/test" } };
		Assert.IsTrue(OpenTelemetryExtension.FilterPath(context));
	}
	
	[TestMethod]
	public void FilterPathTestSkipHealth()
	{
		var context = new DefaultHttpContext { Request = { Path = "/api/health" } };
		Assert.IsFalse(OpenTelemetryExtension.FilterPath(context));
	}
	
	[TestMethod]
	public void FilterPathTestSkipRealtime()
	{
		var context = new DefaultHttpContext { Request = { Path = "/realtime" } };
		Assert.IsFalse(OpenTelemetryExtension.FilterPath(context));
	}
}
