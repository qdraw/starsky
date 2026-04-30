using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.accountmanagement.Helpers;
using starsky.foundation.accountmanagement.Interfaces;
using starsky.foundation.accountmanagement.Middleware;
using starsky.foundation.accountmanagement.Services;

namespace starskytest.starsky.foundation.accountmanagement.Middleware;

[TestClass]
public class TenantPathPrefixMiddlewareTest
{
	[TestMethod]
	public async Task Invoke_SingleSegmentTenantSlug_RewritesToRootAndStoresSlug()
	{
		var serviceProvider = CreateServiceProvider();
		var context = CreateContext(serviceProvider, "/main/");
		var nextCalled = false;
		var middleware = new TenantPathPrefixMiddleware(_ =>
		{
			nextCalled = true;
			return Task.CompletedTask;
		});

		await middleware.Invoke(context);

		Assert.IsTrue(nextCalled);
		Assert.AreEqual("/", context.Request.Path.Value);
		Assert.AreEqual("main", context.Items[TenantAuthenticationConstants.TenantSlugItemKey]);
	}

	[TestMethod]
	public async Task Invoke_SingleSegmentTenantSlugWithoutTrailingSlash_RewritesToRootAndStoresSlug()
	{
		var serviceProvider = CreateServiceProvider();
		var context = CreateContext(serviceProvider, "/main");
		var nextCalled = false;
		var middleware = new TenantPathPrefixMiddleware(_ =>
		{
			nextCalled = true;
			return Task.CompletedTask;
		});

		await middleware.Invoke(context);

		Assert.IsTrue(nextCalled);
		Assert.AreEqual("/", context.Request.Path.Value);
		Assert.AreEqual("main", context.Items[TenantAuthenticationConstants.TenantSlugItemKey]);
	}

	[TestMethod]
	public async Task Invoke_ReservedSegmentPath_RequiresTenantAndReturns404()
	{
		var serviceProvider = CreateServiceProvider();
		var context = CreateContext(serviceProvider, "/search");
		var nextCalled = false;
		var middleware = new TenantPathPrefixMiddleware(_ =>
		{
			nextCalled = true;
			return Task.CompletedTask;
		});

		await middleware.Invoke(context);

		Assert.AreEqual(StatusCodes.Status404NotFound, context.Response.StatusCode);
		Assert.IsFalse(nextCalled);
		Assert.IsFalse(context.Items.ContainsKey(TenantAuthenticationConstants.TenantSlugItemKey));
	}

	[TestMethod]
	public async Task Invoke_ApiPathWithoutTenant_IsNotTreatedAsTenantSlug()
	{
		var serviceProvider = CreateServiceProvider();
		var context = CreateContext(serviceProvider, "/api/account/status");
		var nextCalled = false;
		var middleware = new TenantPathPrefixMiddleware(_ =>
		{
			nextCalled = true;
			return Task.CompletedTask;
		});

		await middleware.Invoke(context);

		Assert.AreEqual(StatusCodes.Status404NotFound, context.Response.StatusCode);
		Assert.IsFalse(nextCalled);
		Assert.IsFalse(context.Items.ContainsKey(TenantAuthenticationConstants.TenantSlugItemKey));
	}

	[TestMethod]
	public async Task Invoke_MultiSegmentTenantSlug_RewritesPathAndStoresSlug()
	{
		var serviceProvider = CreateServiceProvider();
		var context = CreateContext(serviceProvider, "/main/search");
		var nextCalled = false;
		var middleware = new TenantPathPrefixMiddleware(_ =>
		{
			nextCalled = true;
			return Task.CompletedTask;
		});

		await middleware.Invoke(context);

		Assert.IsTrue(nextCalled);
		Assert.AreEqual("/search", context.Request.Path.Value);
		Assert.AreEqual("main", context.Items[TenantAuthenticationConstants.TenantSlugItemKey]);
	}

	private static IServiceProvider CreateServiceProvider()
	{
		var services = new ServiceCollection();
		services.AddSingleton<ITenantSlugValidator>(new TenantSlugValidator());
		return services.BuildServiceProvider();
	}

	private static HttpContext CreateContext(IServiceProvider serviceProvider, string path)
	{
		var context = new DefaultHttpContext
		{
			RequestServices = serviceProvider
		};
		context.Request.Path = path;
		return context;
	}
}


