using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.packagetelemetry.Interfaces;
using starsky.Helpers;
using starskytest.FakeMocks;

namespace starskytest.Helpers;

[TestClass]
public class TimeApplicationRunningTests
{
	[TestMethod]
	public async Task SetRunningTime_ShouldCallAddOrUpdateApplicationStopping()
	{
		// Arrange
		var services = new ServiceCollection();
		var fakeDiagnosticsService = new FakeLifetimeDiagnosticsService();
		var cancellationTokenSource = new CancellationTokenSource();
		services.AddSingleton<IHostApplicationLifetime>(
			new FakeHostApplicationLifetime(cancellationTokenSource.Token));
		services.AddSingleton<ILifetimeDiagnosticsService>(fakeDiagnosticsService);
		var serviceProvider = services.BuildServiceProvider();

		var appBuilder = new FakeApplicationBuilder(serviceProvider);

		// Act
		TimeApplicationRunning.SetRunningTime(appBuilder, DateTime.UtcNow.AddMinutes(-5));

		// Simulate application stopping
		await cancellationTokenSource.CancelAsync();
		cancellationTokenSource.Dispose();

		// Assert
		var time = await fakeDiagnosticsService.GetLastApplicationStoppingTimeInMinutes();

		Console.WriteLine(time);

		Assert.IsTrue(time > 0.00000000001);
		Assert.IsTrue(time < 1000);
	}

	private sealed class FakeHostApplicationLifetime(CancellationToken applicationStopping)
		: IHostApplicationLifetime
	{
		public CancellationToken ApplicationStarted => CancellationToken.None;
		public CancellationToken ApplicationStopped => CancellationToken.None;
		public CancellationToken ApplicationStopping { get; } = applicationStopping;

		public void StopApplication()
		{
			throw new NotImplementedException();
		}
	}

	private sealed class FakeApplicationBuilder(IServiceProvider serviceProvider)
		: IApplicationBuilder
	{
		public IApplicationBuilder Use(Func<RequestDelegate, RequestDelegate> middleware)
		{
			throw new NotImplementedException();
		}

		public IApplicationBuilder New()
		{
			throw new NotImplementedException();
		}

		public RequestDelegate Build()
		{
			throw new NotImplementedException();
		}

		public IServiceProvider ApplicationServices { get; set; } = serviceProvider;
		public IFeatureCollection? ServerFeatures { get; } = new FeatureCollection();

		public IDictionary<string, object?>? Properties { get; } =
			new Dictionary<string, object?>();
	}
}
