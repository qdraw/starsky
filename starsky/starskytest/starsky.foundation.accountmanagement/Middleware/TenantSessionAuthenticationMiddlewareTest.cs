using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.accountmanagement.Helpers;
using starsky.foundation.accountmanagement.Interfaces;
using starsky.foundation.accountmanagement.Middleware;
using starsky.foundation.database.Models.Account;

namespace starskytest.starsky.foundation.accountmanagement.Middleware;

[TestClass]
public class TenantSessionAuthenticationMiddlewareTest : DatabaseTest
{
	[TestMethod]
	public async Task Invoke_AuthorizedTenantApi_WithoutSessionCookie_Returns401()
	{
		var sessionStore = new FakeTenantSessionStore();
		var serviceProvider = CreateServiceProvider(sessionStore);
		var context = CreateContext(serviceProvider, "/api/account/status", true, "main");
		var nextCalled = false;
		var middleware = new TenantSessionAuthenticationMiddleware(_ =>
		{
			nextCalled = true;
			return Task.CompletedTask;
		});

		await middleware.Invoke(context);

		Assert.AreEqual(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
		Assert.IsFalse(nextCalled);
	}

	[TestMethod]
	[DataRow(false, true, true)]
	[DataRow(true, false, true)]
	[DataRow(true, true, false)]
	public async Task Invoke_AuthorizedTenantApi_InvalidTenantAuthorization_Returns403(
		bool tenantEnabled, bool tenantActivated, bool hasMembership)
	{
		var user = new User { Name = "tenant-user", Created = DateTime.UtcNow };
		await DbContext.Users.AddAsync(user, TestContext.CancellationTokenSource.Token);

		var tenant = new Tenant
		{
			Slug = "main",
			Name = "main",
			IsEnabled = tenantEnabled,
			Created = DateTime.UtcNow
		};
		await DbContext.Tenants.AddAsync(tenant, TestContext.CancellationTokenSource.Token);
		await DbContext.SaveChangesAsync(TestContext.CancellationTokenSource.Token);

		if (hasMembership)
		{
			await DbContext.TenantUsers.AddAsync(new TenantUser
			{
				TenantId = tenant.Id,
				UserId = user.Id,
				Role = TenantRole.Admin,
				Created = DateTime.UtcNow
			}, TestContext.CancellationTokenSource.Token);
			await DbContext.SaveChangesAsync(TestContext.CancellationTokenSource.Token);
		}

		var session = new WebSession
		{
			Id = 7331,
			SessionId = "test-session-id",
			UserId = user.Id,
			Created = DateTime.UtcNow,
			LastSeen = DateTime.UtcNow,
			ExpiresAt = DateTime.UtcNow.AddHours(1)
		};
		var sessionStore = new FakeTenantSessionStore
		{
			Session = session,
			TenantActivated = tenantActivated
		};

		var serviceProvider = CreateServiceProvider(sessionStore);
		var context = CreateContext(serviceProvider, "/api/account/status", true, tenant.Slug,
			TenantAuthenticationConstants.SessionCookieName + "=" + session.SessionId);
		var nextCalled = false;
		var middleware = new TenantSessionAuthenticationMiddleware(_ =>
		{
			nextCalled = true;
			return Task.CompletedTask;
		});

		await middleware.Invoke(context);

		Assert.AreEqual(StatusCodes.Status403Forbidden, context.Response.StatusCode);
		Assert.IsFalse(nextCalled);
	}

	private IServiceProvider CreateServiceProvider(ITenantSessionStore sessionStore)
	{
		var services = new ServiceCollection();
		services.AddSingleton(DbContext);
		services.AddSingleton(sessionStore);
		return services.BuildServiceProvider();
	}

	private static HttpContext CreateContext(IServiceProvider serviceProvider, string path,
		bool authorizedEndpoint, string tenantSlug, string? cookieHeader = null)
	{
		var context = new DefaultHttpContext
		{
			RequestServices = serviceProvider
		};
		context.Request.Path = path;
		context.Items[TenantAuthenticationConstants.TenantSlugItemKey] = tenantSlug;
		if (!string.IsNullOrWhiteSpace(cookieHeader))
		{
			context.Request.Headers.Cookie = cookieHeader;
		}

		var endpoint = authorizedEndpoint
			? new Endpoint(_ => Task.CompletedTask,
				new EndpointMetadataCollection(new AuthorizeAttribute()),
				"authorized")
			: new Endpoint(_ => Task.CompletedTask,
				new EndpointMetadataCollection(),
				"anonymous");

		context.SetEndpoint(endpoint);
		return context;
	}

	private sealed class FakeTenantSessionStore : ITenantSessionStore
	{
		public WebSession? Session { get; set; }
		public bool TenantActivated { get; set; } = true;

		public Task<WebSession?> GetValidSessionAsync(string sessionId)
		{
			if (Session?.SessionId == sessionId)
			{
				return Task.FromResult<WebSession?>(Session);
			}

			return Task.FromResult<WebSession?>(null);
		}

		public Task<WebSession> CreateOrRefreshSessionAsync(int userId, string? existingSessionId = null)
		{
			throw new NotImplementedException();
		}

		public Task<bool> ActivateTenantAsync(int webSessionId, int tenantId)
		{
			throw new NotImplementedException();
		}

		public Task<bool> DeactivateTenantAsync(int webSessionId, int tenantId)
		{
			throw new NotImplementedException();
		}

		public Task RevokeSessionAsync(int webSessionId)
		{
			throw new NotImplementedException();
		}

		public Task<bool> IsTenantActivatedAsync(int webSessionId, int tenantId)
		{
			return Task.FromResult(TenantActivated);
		}

		public Task TouchSessionAsync(int webSessionId, int tenantId)
		{
			return Task.CompletedTask;
		}
	}

	public TestContext TestContext { get; set; }
}
