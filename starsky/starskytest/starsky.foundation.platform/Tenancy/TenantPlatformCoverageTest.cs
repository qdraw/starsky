using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Tenancy;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.platform.Tenancy;

[TestClass]
public sealed class TenantPlatformCoverageTest
{
	public TestContext TestContext { get; set; } = null!;

	private readonly string _root =
		Path.Combine(Path.GetTempPath(), "starsky-tenant-cov-" + Guid.NewGuid().ToString("N"));

	[TestCleanup]
	public void Cleanup()
	{
		TenantAmbient.Reset();
		if ( Directory.Exists(_root) )
		{
			Directory.Delete(_root, true);
		}
	}

	private ScopedTenantSettings CreateScoped()
	{
		var appSettings = new AppSettings
		{
			StorageFolder = Path.Combine(_root, "storage"),
			ThumbnailTempFolder = Path.Combine(_root, "thumbs"),
			TempFolder = Path.Combine(_root, "temp")
		};
		var provider = new TenantSettingsProvider(appSettings, new TenantRegistry());
		return new ScopedTenantSettings(new TenantContext(), provider);
	}

	[TestMethod]
	public void ScopedTenantSettings_DelegatesToTenantOfContext()
	{
		var settings = CreateScoped();

		Assert.AreEqual("default", settings.TenantId);
		Assert.AreEqual(Path.Combine(_root, "storage"),
			settings.StorageFolder.TrimEnd(Path.DirectorySeparatorChar));
		Assert.AreEqual(Path.Combine(_root, "thumbs"),
			settings.ThumbnailTempFolder.TrimEnd(Path.DirectorySeparatorChar));
		Assert.AreEqual(Path.Combine(_root, "temp"),
			settings.TempFolder.TrimEnd(Path.DirectorySeparatorChar));
		Assert.IsNotNull(settings.StorageFolderMappings);
		Assert.IsFalse(settings.IsReadOnly("/not-read-only"));
		Assert.EndsWith("a.jpg", settings.DatabasePathToFilePath("/a.jpg"));
		Assert.StartsWith(Path.Combine(_root, "storage"),
			settings.DatabasePathToFilePath("/a.jpg"));
		Assert.EndsWith("a.jpg", settings.DatabasePathToTempFolderFilePath("/a.jpg"));
		Assert.StartsWith(Path.Combine(_root, "temp"),
			settings.DatabasePathToTempFolderFilePath("/a.jpg"));
		Assert.AreEqual("/a.jpg",
			settings.FullPathStorageFolderToDatabaseStyle(
				Path.Combine(_root, "storage", "a.jpg")));
		Assert.AreSequenceEqual(new List<string> { "/a.jpg" },
			settings.RenameListItemsToDbStyle([Path.Combine(_root, "storage", "a.jpg")]));
		var withDate = settings.RenameListItemsToDbStyle(
			[new KeyValuePair<string, DateTime>(Path.Combine(_root, "storage", "a.jpg"),
				DateTime.UnixEpoch)]).ToList();
		Assert.AreEqual("/a.jpg", withDate.Single().Key);
		Assert.AreSequenceEqual(new List<string> { "/a.jpg" },
			settings.RenameTempListItemsToDbStyle([Path.Combine(_root, "temp", "a.jpg")]));
	}

	[TestMethod]
	public void TenantRegistry_OnlyDefault()
	{
		var registry = new TenantRegistry();
		Assert.IsNull(registry.GetRoots("acme"));
		registry.Invalidate();
		Assert.IsTrue(registry.IsActive("default"));
		Assert.IsFalse(registry.IsActive("acme"));
	}

	[TestMethod]
	public void TenantSettings_InvalidId_Throws()
	{
		Assert.ThrowsExactly<ArgumentException>(() =>
			new TenantSettings("Not Valid!", new AppSettings()));
	}

	[TestMethod]
	public void TenantSettingsProvider_Build_InvalidId_Throws()
	{
		var provider = new TenantSettingsProvider(new AppSettings(), new TenantRegistry());
		Assert.ThrowsExactly<ArgumentException>(() => provider.Build("../x", null));
	}

	[TestMethod]
	public async Task TenantJobs_ForTenants_CancelledBeforeStart_RunsNothing()
	{
		var services = new ServiceCollection();
		services.AddScoped<ITenantContext, TenantContext>();
		var provider = services.BuildServiceProvider();
		using var cancellation = new CancellationTokenSource();
		await cancellation.CancelAsync();

		var ran = new List<string>();
		var failures = await TenantJobs.ForTenantsAsync(
			provider.GetRequiredService<IServiceScopeFactory>(), ["default", "acme"],
			(_, tenantId) =>
			{
				ran.Add(tenantId);
				return Task.CompletedTask;
			}, cancellationToken: cancellation.Token);

		Assert.IsEmpty(ran);
		Assert.IsEmpty(failures);
	}

	[TestMethod]
	public async Task TenantJobs_ForTenants_FailureWithoutErrorCallback_IsReturned()
	{
		var services = new ServiceCollection();
		services.AddScoped<ITenantContext, TenantContext>();
		var provider = services.BuildServiceProvider();

		var failures = await TenantJobs.ForTenantsAsync(
			provider.GetRequiredService<IServiceScopeFactory>(), ["default"],
			(_, _) => throw new InvalidOperationException("boom"),
			cancellationToken: TestContext.CancellationToken);

		Assert.HasCount(1, failures);
		Assert.AreEqual("default", failures[0].TenantId);
	}

	[TestMethod]
	public async Task TenantResolution_OnlyPrefixWithoutId_NotFound()
	{
		var services = new ServiceCollection();
		services.AddSingleton<ITenantRegistry>(new FakeTenantRegistry("default", "acme"));
		services.AddScoped<ITenantContext, TenantContext>();
		using var provider = services.BuildServiceProvider();
		using var scope = provider.CreateScope();

		var httpContext = new DefaultHttpContext { RequestServices = scope.ServiceProvider };
		httpContext.Request.Path = "/t";
		var nextCalled = false;
		await new TenantResolutionMiddleware(_ =>
		{
			nextCalled = true;
			return Task.CompletedTask;
		}).Invoke(httpContext);

		Assert.AreEqual(StatusCodes.Status404NotFound, httpContext.Response.StatusCode);
		Assert.IsFalse(nextCalled);
	}
}
