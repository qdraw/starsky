using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.packagetelemetry.Interfaces;
using starsky.feature.packagetelemetry.Services;
using starsky.foundation.database.Interfaces;
using starsky.foundation.http.Interfaces;
using starsky.foundation.http.Services;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.packagetelemetry.Services {

	public static class ExtensionMethods
	{
		public static async Task InvokeAsync(this MethodInfo @this, object obj, params object[] parameters)
		{
			dynamic awaitable = @this.Invoke(obj, parameters);
			await awaitable!;
			awaitable.GetAwaiter();
		}
	}

	[TestClass]
	public sealed class PackageTelemetryBackgroundServiceTest
	{
		private readonly IServiceScopeFactory _serviceScopeFactory;

		public PackageTelemetryBackgroundServiceTest()
		{
			var services = new ServiceCollection();
			services.AddSingleton<AppSettings>();
			services.AddSingleton<IHttpClientHelper, HttpClientHelper>();
			services.AddSingleton<IHttpProvider, FakeIHttpProvider>();
			services.AddSingleton<IWebLogger, FakeIWebLogger>();
			services.AddSingleton<ISelectorStorage, FakeSelectorStorage>();
			services.AddSingleton<IQuery, FakeIQuery>();
			services.AddSingleton<IDeviceIdService, FakeIDeviceIdService>();

			var serviceProvider = services.BuildServiceProvider();
			_serviceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
			
			var appSettings = serviceProvider.GetRequiredService<AppSettings>();
			appSettings.EnablePackageTelemetry = true;
		}
	
	
		[TestMethod]
		[Timeout(5000)]
		public async Task ExecuteAsyncTest_WebController()
		{
			var appSettings = _serviceScopeFactory.CreateScope().ServiceProvider
				.GetService<AppSettings>();
			appSettings!.ApplicationType = AppSettings.StarskyAppType.WebController;
			appSettings.EnablePackageTelemetry = true;
		
			var service = new PackageTelemetryBackgroundService(_serviceScopeFactory);
			
			CancellationTokenSource source = new CancellationTokenSource();
			CancellationToken token = source.Token;
			source.Cancel(); // <- cancel before start

			MethodInfo dynMethod = service.GetType().GetMethod("ExecuteAsync", 
				BindingFlags.NonPublic | BindingFlags.Instance);
			if ( dynMethod == null )
				throw new Exception("missing ExecuteAsync");
			await dynMethod.InvokeAsync(service, new object[]
			{
				token
			});

			var httpProvider = _serviceScopeFactory.CreateScope().ServiceProvider
				.GetService<IHttpProvider>();

			var fakeHttpProvider = httpProvider as FakeIHttpProvider;
			Assert.IsTrue(fakeHttpProvider?.UrlCalled.Any(p => p.Contains(PackageTelemetry.PackageTelemetryUrl)));
		}
	
		[TestMethod]
		[Timeout(2000)]
		public void ExecuteAsyncTest_NotWhenDisabled()
		{
			var appSettings = _serviceScopeFactory.CreateScope().ServiceProvider
				.GetService<AppSettings>();
			appSettings!.ApplicationType = AppSettings.StarskyAppType.Admin;
			var httpProvider1 = _serviceScopeFactory.CreateScope().ServiceProvider
				.GetService<IHttpProvider>();

			var fakeHttpProvider1 = httpProvider1 as FakeIHttpProvider;
			fakeHttpProvider1!.UrlCalled = new List<string>();
		
			var service = new PackageTelemetryBackgroundService(_serviceScopeFactory);
			
			CancellationTokenSource source = new CancellationTokenSource();
			CancellationToken token = source.Token;
			source.Cancel(); // <- cancel before start

			MethodInfo dynMethod = service.GetType().GetMethod("ExecuteAsync", 
				BindingFlags.NonPublic | BindingFlags.Instance);
			if ( dynMethod == null )
				throw new Exception("missing ExecuteAsync");
			dynMethod.Invoke(service, new object[]
			{
				token
			});

			var httpProvider = _serviceScopeFactory.CreateScope().ServiceProvider
				.GetService<IHttpProvider>();

			var fakeHttpProvider = httpProvider as FakeIHttpProvider;
			Assert.IsFalse(fakeHttpProvider?.UrlCalled.Contains(PackageTelemetry.PackageTelemetryUrl));
		}
	}
}
