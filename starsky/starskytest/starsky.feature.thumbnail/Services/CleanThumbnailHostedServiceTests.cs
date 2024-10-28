using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.thumbnail.Services;
using starsky.foundation.platform.Models;
using starsky.foundation.thumbnailgeneration.Interfaces;
using starskytest.ExtensionMethods;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.thumbnail.Services;

[TestClass]
public class CleanThumbnailHostedServiceTest
{
	private static CleanThumbnailHostedService CreateServiceScope(
		bool thumbnailCleanupSkipOnStartup)
	{
		var serviceProvider = new ServiceCollection()
			.AddSingleton<IThumbnailCleaner, FakeIThumbnailCleaner>()
			.AddSingleton<AppSettings>()
			.BuildServiceProvider();

		var service =
			serviceProvider.GetRequiredService<IThumbnailCleaner>() as FakeIThumbnailCleaner;
		service!.Files = ["test.jpg"];

		var appSettings =
			serviceProvider.GetRequiredService<AppSettings>();
		appSettings.ThumbnailCleanupSkipOnStartup = thumbnailCleanupSkipOnStartup;

		var serviceScopeFactory = new FakeServiceScopeFactory(serviceProvider);
		var hostedService = new CleanThumbnailHostedService(serviceScopeFactory);
		return hostedService;
	}

	[TestMethod]
	[Timeout(10000)]
	public async Task ExecuteAsync_ShouldInvokeCleanAllUnusedFilesAsync()
	{
		// Arrange
		var hostedService = CreateServiceScope(false);
		using var cancellationTokenSource = new CancellationTokenSource();
		var stoppingToken = cancellationTokenSource.Token;

		// Act
		var dynMethod = hostedService.GetType().GetMethod("ExecuteAsync",
			                BindingFlags.NonPublic | BindingFlags.Instance) ??
		                throw new Exception("missing ExecuteAsync");

		// Assert
		// The method should throw a TimeoutException if it takes longer than 1 second to execute
		await Assert.ThrowsExceptionAsync<TimeoutException>(async () =>
		{
			await dynMethod.InvokeAsync(hostedService, stoppingToken)
				.WaitAsync(TimeSpan.FromSeconds(1), new CancellationToken());
		});
	}

	[TestMethod]
	[Timeout(5000)]
	public async Task StartBackgroundAsync_HappyFlow()
	{
		// Arrange
		var hostedService = CreateServiceScope(false);
		using var cancellationTokenSource = new CancellationTokenSource();
		var stoppingToken = cancellationTokenSource.Token;

		// Act
		var result = await hostedService.StartBackgroundAsync(new TimeSpan(0), stoppingToken);
		// mock always return one item
		Assert.AreEqual(1, result.Count);
	}

	[TestMethod]
	[Timeout(5000)]
	public async Task StartBackgroundAsync_Disabled()
	{
		// Arrange
		var hostedService = CreateServiceScope(true);
		using var cancellationTokenSource = new CancellationTokenSource();
		var stoppingToken = cancellationTokenSource.Token;

		// Act
		var result = await hostedService.StartBackgroundAsync(new TimeSpan(0), stoppingToken);
		// mock always return one item, except when disabled
		Assert.AreEqual(0, result.Count);
	}

	private class FakeServiceScope : IServiceScope
	{
		public FakeServiceScope(IServiceProvider serviceProvider)
		{
			ServiceProvider = serviceProvider;
		}

		public IServiceProvider ServiceProvider { get; }

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			// Cleanup
		}
	}

	private class FakeServiceScopeFactory : IServiceScopeFactory
	{
		private readonly IServiceProvider _serviceProvider;

		public FakeServiceScopeFactory(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}

		public IServiceScope CreateScope()
		{
			return new FakeServiceScope(_serviceProvider);
		}
	}
}
