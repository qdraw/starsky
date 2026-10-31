using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Tenancy;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.platform.Tenancy;

[TestClass]
public sealed class TenantResolutionMiddlewareTest
{
	private sealed record Result(
		int StatusCode,
		bool NextCalled,
		string TenantId,
		string Path,
		string PathBase);

	private static async Task<Result> Run(string path, string pathBase = "/starsky")
	{
		var services = new ServiceCollection();
		services.AddSingleton<ITenantRegistry>(new FakeTenantRegistry("default", "acme"));
		services.AddScoped<ITenantContext, TenantContext>();
		var provider = services.BuildServiceProvider();
		using var scope = provider.CreateScope();

		var httpContext = new DefaultHttpContext { RequestServices = scope.ServiceProvider };
		httpContext.Request.PathBase = pathBase;
		httpContext.Request.Path = path;

		var nextCalled = false;
		var middleware = new TenantResolutionMiddleware(_ =>
		{
			nextCalled = true;
			return Task.CompletedTask;
		});
		await middleware.Invoke(httpContext);

		return new Result(httpContext.Response.StatusCode, nextCalled,
			scope.ServiceProvider.GetRequiredService<ITenantContext>().TenantId,
			httpContext.Request.Path, httpContext.Request.PathBase);
	}

	[TestMethod]
	public async Task NoPrefix_IsDefaultTenant()
	{
		var result = await Run("/api/index");

		Assert.IsTrue(result.NextCalled);
		Assert.AreEqual("default", result.TenantId);
		Assert.AreEqual("/api/index", result.Path);
		Assert.AreEqual("/starsky", result.PathBase);
	}

	[TestMethod]
	public async Task Prefix_SetsTenant_AndMovesPrefixToPathBase()
	{
		var result = await Run("/t/acme/api/index");

		Assert.IsTrue(result.NextCalled);
		Assert.AreEqual("acme", result.TenantId);
		Assert.AreEqual("/api/index", result.Path);
		Assert.AreEqual("/starsky/t/acme", result.PathBase);
	}

	[TestMethod]
	public async Task Prefix_OnlyTenant_PathIsEmpty()
	{
		var result = await Run("/t/acme");

		Assert.IsTrue(result.NextCalled);
		Assert.AreEqual("acme", result.TenantId);
		Assert.AreEqual(string.Empty, result.Path);
	}

	[TestMethod]
	[DataRow("/t/unknown/api")]
	[DataRow("/t/Acme/api")]
	[DataRow("/t/../api")]
	[DataRow("/t/")]
	[DataRow("/t")]
	public async Task Prefix_UnknownOrInvalid_NotFound(string path)
	{
		var result = await Run(path);

		Assert.IsFalse(result.NextCalled);
		Assert.AreEqual(404, result.StatusCode);
	}

	[TestMethod]
	public async Task SimilarSegment_IsNotAPrefix()
	{
		var result = await Run("/tenants/acme");

		Assert.IsTrue(result.NextCalled);
		Assert.AreEqual("default", result.TenantId);
		Assert.AreEqual("/tenants/acme", result.Path);
	}

	[TestMethod]
	public void TenantContext_InvalidId_Throws()
	{
		Assert.ThrowsExactly<System.ArgumentException>(() =>
			new TenantContext().SetTenant("../x"));
	}

	[TestMethod]
	public void TenantRegistry_OnlyDefaultActive()
	{
		var registry = new TenantRegistry();

		Assert.IsTrue(registry.IsActive("default"));
		Assert.IsFalse(registry.IsActive("acme"));
	}
}
