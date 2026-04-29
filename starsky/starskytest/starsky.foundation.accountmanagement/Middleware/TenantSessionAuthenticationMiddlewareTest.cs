using System;
using System.Security.Claims;
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
	public async Task Invoke_WithoutTenantSlug_AllowsRequest()
	{
		var sessionStore = new FakeTenantSessionStore();
		var serviceProvider = CreateServiceProvider(sessionStore);
		var context = CreateContext(serviceProvider, "/api/account/status", true, null);
		var nextCalled = false;
		var middleware = new TenantSessionAuthenticationMiddleware(_ =>
		{
			nextCalled = true;
			return Task.CompletedTask;
		});

		await middleware.Invoke(context);

		Assert.IsTrue(nextCalled);
	}

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
	public async Task Invoke_AuthorizedTenantApi_InvalidSession_Returns401()
	{
		var sessionStore = new FakeTenantSessionStore();
		var serviceProvider = CreateServiceProvider(sessionStore);
		var context = CreateContext(serviceProvider, "/api/account/status", true, "main",
			TenantAuthenticationConstants.SessionCookieName + "=unknown-session");
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
	public async Task Invoke_TenantApi_AnonymousEndpoint_WithoutSessionCookie_AllowsRequest()
	{
		var sessionStore = new FakeTenantSessionStore();
		var serviceProvider = CreateServiceProvider(sessionStore);
		var context = CreateContext(serviceProvider, "/api/account/status", false, "main");
		var nextCalled = false;
		var middleware = new TenantSessionAuthenticationMiddleware(_ =>
		{
			nextCalled = true;
			return Task.CompletedTask;
		});

		await middleware.Invoke(context);

		Assert.IsTrue(nextCalled);
	}

	[TestMethod]
	public async Task Invoke_GlobalTenantsMine_WithoutSessionCookie_Returns401()
	{
		var sessionStore = new FakeTenantSessionStore();
		var serviceProvider = CreateServiceProvider(sessionStore);
		var context = CreateContext(serviceProvider, "/api/tenants/mine", true, null);
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
	public async Task Invoke_GlobalTenantsMine_InvalidSession_Returns401()
	{
		var sessionStore = new FakeTenantSessionStore();
		var serviceProvider = CreateServiceProvider(sessionStore);
		var context = CreateContext(serviceProvider, "/api/tenants/mine", true, null,
			TenantAuthenticationConstants.SessionCookieName + "=unknown-session");
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

	[TestMethod]
	public async Task Invoke_GlobalTenantsMine_WithValidSession_AddsPrincipalAndAllowsRequest()
	{
		var user = new User
		{
			Name = "global-user",
			Created = DateTime.UtcNow,
			IsGlobalAdmin = true
		};
		await DbContext.Users.AddAsync(user, TestContext.CancellationTokenSource.Token);
		await DbContext.SaveChangesAsync(TestContext.CancellationTokenSource.Token);

		var session = new WebSession
		{
			Id = 9901,
			SessionId = "global-session",
			UserId = user.Id,
			Created = DateTime.UtcNow,
			LastSeen = DateTime.UtcNow,
			ExpiresAt = DateTime.UtcNow.AddHours(1)
		};
		var sessionStore = new FakeTenantSessionStore { Session = session };
		var serviceProvider = CreateServiceProvider(sessionStore);
		var context = CreateContext(serviceProvider, "/api/tenants/mine", true, null,
			TenantAuthenticationConstants.SessionCookieName + "=" + session.SessionId);
		var nextCalled = false;
		var middleware = new TenantSessionAuthenticationMiddleware(_ =>
		{
			nextCalled = true;
			return Task.CompletedTask;
		});

		await middleware.Invoke(context);

		Assert.IsTrue(nextCalled);
		Assert.AreEqual(user.Id.ToString(),
			context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
		Assert.AreEqual("true",
			context.User.FindFirst(TenantAuthenticationConstants.GlobalAdminClaimType)?.Value);
	}

	[TestMethod]
	public async Task Invoke_GlobalTenantsMine_WithSessionButUnknownUser_Returns401()
	{
		var session = new WebSession
		{
			Id = 9101,
			SessionId = "missing-user-session",
			UserId = 999999,
			Created = DateTime.UtcNow,
			LastSeen = DateTime.UtcNow,
			ExpiresAt = DateTime.UtcNow.AddHours(1)
		};
		var sessionStore = new FakeTenantSessionStore { Session = session };
		var serviceProvider = CreateServiceProvider(sessionStore);
		var context = CreateContext(serviceProvider, "/api/tenants/mine", true, null,
			TenantAuthenticationConstants.SessionCookieName + "=" + session.SessionId);
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
	public async Task Invoke_WithCookieAndMembership_AddsTenantClaimsAndTouchesSession()
	{
		var user = new User
		{
			Name = "member-user",
			Created = DateTime.UtcNow,
			IsGlobalAdmin = false
		};
		await DbContext.Users.AddAsync(user, TestContext.CancellationTokenSource.Token);

		var tenant = new Tenant
		{
			Slug = "main",
			Name = "main",
			IsEnabled = true,
			Created = DateTime.UtcNow
		};
		await DbContext.Tenants.AddAsync(tenant, TestContext.CancellationTokenSource.Token);
		await DbContext.SaveChangesAsync(TestContext.CancellationTokenSource.Token);

		await DbContext.TenantUsers.AddAsync(new TenantUser
		{
			TenantId = tenant.Id,
			UserId = user.Id,
			Role = TenantRole.Admin,
			Created = DateTime.UtcNow
		}, TestContext.CancellationTokenSource.Token);
		await DbContext.SaveChangesAsync(TestContext.CancellationTokenSource.Token);

		var session = new WebSession
		{
			Id = 8008,
			SessionId = "active-session",
			UserId = user.Id,
			Created = DateTime.UtcNow,
			LastSeen = DateTime.UtcNow,
			ExpiresAt = DateTime.UtcNow.AddHours(1)
		};
		var sessionStore = new FakeTenantSessionStore
		{
			Session = session,
			TenantActivated = true
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

		Assert.IsTrue(nextCalled);
		Assert.AreEqual(1, sessionStore.TouchCalls);
		Assert.AreEqual(tenant.Slug,
			context.User.FindFirst(TenantAuthenticationConstants.TenantSlugClaimType)?.Value);
		Assert.AreEqual(TenantRole.Admin.ToString(),
			context.User.FindFirst(TenantAuthenticationConstants.TenantRoleClaimType)?.Value);
	}

	[TestMethod]
	public async Task Invoke_WithExistingAuthenticatedPrincipalAndMembership_UsesPrincipalEnrichment()
	{
		var user = new User
		{
			Name = "basic-user",
			Created = DateTime.UtcNow,
			IsGlobalAdmin = false
		};
		await DbContext.Users.AddAsync(user, TestContext.CancellationTokenSource.Token);

		var tenant = new Tenant
		{
			Slug = "main",
			Name = "main",
			IsEnabled = true,
			Created = DateTime.UtcNow
		};
		await DbContext.Tenants.AddAsync(tenant, TestContext.CancellationTokenSource.Token);
		await DbContext.SaveChangesAsync(TestContext.CancellationTokenSource.Token);

		await DbContext.TenantUsers.AddAsync(new TenantUser
		{
			TenantId = tenant.Id,
			UserId = user.Id,
			Role = TenantRole.User,
			Created = DateTime.UtcNow
		}, TestContext.CancellationTokenSource.Token);
		await DbContext.SaveChangesAsync(TestContext.CancellationTokenSource.Token);

		var sessionStore = new FakeTenantSessionStore();
		var serviceProvider = CreateServiceProvider(sessionStore);
		var context = CreateContext(serviceProvider, "/api/account/status", true, tenant.Slug);
		context.User = CreateAuthenticatedPrincipal(user.Id);

		var nextCalled = false;
		var middleware = new TenantSessionAuthenticationMiddleware(_ =>
		{
			nextCalled = true;
			return Task.CompletedTask;
		});

		await middleware.Invoke(context);

		Assert.IsTrue(nextCalled);
		Assert.AreEqual(tenant.Slug,
			context.User.FindFirst(TenantAuthenticationConstants.TenantSlugClaimType)?.Value);
		Assert.AreEqual(TenantRole.User.ToString(),
			context.User.FindFirst(TenantAuthenticationConstants.TenantRoleClaimType)?.Value);
	}

	[TestMethod]
	public async Task Invoke_WithExistingAuthenticatedPrincipalButNoMembership_Returns403()
	{
		var user = new User
		{
			Name = "basic-user-no-membership",
			Created = DateTime.UtcNow,
			IsGlobalAdmin = false
		};
		await DbContext.Users.AddAsync(user, TestContext.CancellationTokenSource.Token);

		var tenant = new Tenant
		{
			Slug = "main",
			Name = "main",
			IsEnabled = true,
			Created = DateTime.UtcNow
		};
		await DbContext.Tenants.AddAsync(tenant, TestContext.CancellationTokenSource.Token);
		await DbContext.SaveChangesAsync(TestContext.CancellationTokenSource.Token);

		var sessionStore = new FakeTenantSessionStore();
		var serviceProvider = CreateServiceProvider(sessionStore);
		var context = CreateContext(serviceProvider, "/api/account/status", true, tenant.Slug);
		context.User = CreateAuthenticatedPrincipal(user.Id);

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

	[TestMethod]
	public async Task Invoke_WithExistingAuthenticatedPrincipalTrySetFailsOnNonApi_AllowsRequest()
	{
		var user = new User
		{
			Name = "existing-nonapi",
			Created = DateTime.UtcNow,
			IsGlobalAdmin = false
		};
		await DbContext.Users.AddAsync(user, TestContext.CancellationTokenSource.Token);
		await DbContext.SaveChangesAsync(TestContext.CancellationTokenSource.Token);

		var serviceProvider = CreateServiceProvider(new FakeTenantSessionStore());
		var context = CreateContext(serviceProvider, "/account/status", true, "main");
		context.User = CreateAuthenticatedPrincipal(user.Id);

		var nextCalled = false;
		var middleware = new TenantSessionAuthenticationMiddleware(_ =>
		{
			nextCalled = true;
			return Task.CompletedTask;
		});

		await middleware.Invoke(context);

		Assert.IsTrue(nextCalled);
	}

	[TestMethod]
	public async Task Invoke_InvalidSession_AnonymousEndpoint_AllowsRequest()
	{
		var sessionStore = new FakeTenantSessionStore();
		var serviceProvider = CreateServiceProvider(sessionStore);
		var context = CreateContext(serviceProvider, "/account/status", false, "main",
			TenantAuthenticationConstants.SessionCookieName + "=unknown-session");

		var nextCalled = false;
		var middleware = new TenantSessionAuthenticationMiddleware(_ =>
		{
			nextCalled = true;
			return Task.CompletedTask;
		});

		await middleware.Invoke(context);

		Assert.IsTrue(nextCalled);
	}

	[TestMethod]
	public async Task Invoke_TenantNotActivated_AnonymousEndpoint_AllowsRequest()
	{
		var user = new User
		{
			Name = "tenant-not-active-anon",
			Created = DateTime.UtcNow
		};
		await DbContext.Users.AddAsync(user, TestContext.CancellationTokenSource.Token);

		var tenant = new Tenant
		{
			Slug = "main",
			Name = "main",
			IsEnabled = true,
			Created = DateTime.UtcNow
		};
		await DbContext.Tenants.AddAsync(tenant, TestContext.CancellationTokenSource.Token);
		await DbContext.SaveChangesAsync(TestContext.CancellationTokenSource.Token);

		await DbContext.TenantUsers.AddAsync(new TenantUser
		{
			TenantId = tenant.Id,
			UserId = user.Id,
			Role = TenantRole.User,
			Created = DateTime.UtcNow
		}, TestContext.CancellationTokenSource.Token);
		await DbContext.SaveChangesAsync(TestContext.CancellationTokenSource.Token);

		var session = new WebSession
		{
			Id = 9601,
			SessionId = "not-activated-anon",
			UserId = user.Id,
			Created = DateTime.UtcNow,
			LastSeen = DateTime.UtcNow,
			ExpiresAt = DateTime.UtcNow.AddHours(1)
		};
		var sessionStore = new FakeTenantSessionStore { Session = session, TenantActivated = false };
		var serviceProvider = CreateServiceProvider(sessionStore);
		var context = CreateContext(serviceProvider, "/account/status", false, tenant.Slug,
			TenantAuthenticationConstants.SessionCookieName + "=" + session.SessionId);

		var nextCalled = false;
		var middleware = new TenantSessionAuthenticationMiddleware(_ =>
		{
			nextCalled = true;
			return Task.CompletedTask;
		});

		await middleware.Invoke(context);

		Assert.IsTrue(nextCalled);
	}

	[TestMethod]
	public async Task Invoke_WithExistingAuthenticatedPrincipalInvalidUserId_Returns403()
	{
		var tenant = new Tenant
		{
			Slug = "main",
			Name = "main",
			IsEnabled = true,
			Created = DateTime.UtcNow
		};
		await DbContext.Tenants.AddAsync(tenant, TestContext.CancellationTokenSource.Token);
		await DbContext.SaveChangesAsync(TestContext.CancellationTokenSource.Token);

		var serviceProvider = CreateServiceProvider(new FakeTenantSessionStore());
		var context = CreateContext(serviceProvider, "/api/account/status", true, tenant.Slug);
		var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "not-an-int") };
		context.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Basic"));

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

	[TestMethod]
	public async Task Invoke_TenantMissing_AnonymousEndpoint_AllowsRequest()
	{
		var user = new User { Name = "tenant-missing-user", Created = DateTime.UtcNow };
		await DbContext.Users.AddAsync(user, TestContext.CancellationTokenSource.Token);
		await DbContext.SaveChangesAsync(TestContext.CancellationTokenSource.Token);

		var session = new WebSession
		{
			Id = 9301,
			SessionId = "tenant-missing-session",
			UserId = user.Id,
			Created = DateTime.UtcNow,
			LastSeen = DateTime.UtcNow,
			ExpiresAt = DateTime.UtcNow.AddHours(1)
		};
		var sessionStore = new FakeTenantSessionStore { Session = session, TenantActivated = true };
		var serviceProvider = CreateServiceProvider(sessionStore);
		var context = CreateContext(serviceProvider, "/api/account/status", false, "main",
			TenantAuthenticationConstants.SessionCookieName + "=" + session.SessionId);
		var nextCalled = false;
		var middleware = new TenantSessionAuthenticationMiddleware(_ =>
		{
			nextCalled = true;
			return Task.CompletedTask;
		});

		await middleware.Invoke(context);

		Assert.IsTrue(nextCalled);
	}

	[TestMethod]
	public async Task Invoke_MembershipMissing_AnonymousEndpoint_AllowsRequest()
	{
		var user = new User { Name = "no-membership-anon", Created = DateTime.UtcNow };
		await DbContext.Users.AddAsync(user, TestContext.CancellationTokenSource.Token);
		var tenant = new Tenant
		{
			Slug = "main",
			Name = "main",
			IsEnabled = true,
			Created = DateTime.UtcNow
		};
		await DbContext.Tenants.AddAsync(tenant, TestContext.CancellationTokenSource.Token);
		await DbContext.SaveChangesAsync(TestContext.CancellationTokenSource.Token);

		var session = new WebSession
		{
			Id = 9401,
			SessionId = "no-membership-anon-session",
			UserId = user.Id,
			Created = DateTime.UtcNow,
			LastSeen = DateTime.UtcNow,
			ExpiresAt = DateTime.UtcNow.AddHours(1)
		};
		var sessionStore = new FakeTenantSessionStore { Session = session, TenantActivated = true };
		var serviceProvider = CreateServiceProvider(sessionStore);
		var context = CreateContext(serviceProvider, "/api/account/status", false, tenant.Slug,
			TenantAuthenticationConstants.SessionCookieName + "=" + session.SessionId);
		var nextCalled = false;
		var middleware = new TenantSessionAuthenticationMiddleware(_ =>
		{
			nextCalled = true;
			return Task.CompletedTask;
		});

		await middleware.Invoke(context);

		Assert.IsTrue(nextCalled);
	}

	private IServiceProvider CreateServiceProvider(ITenantSessionStore sessionStore)
	{
		[TestMethod]
		public async Task Invoke_WithExistingAuthenticatedPrincipalUnknownTenant_Returns403()
		{
			var user = new User
			{
				Name = "existing-unknown-tenant",
				Created = DateTime.UtcNow
			};
			await DbContext.Users.AddAsync(user, TestContext.CancellationTokenSource.Token);
			await DbContext.SaveChangesAsync(TestContext.CancellationTokenSource.Token);

			var serviceProvider = CreateServiceProvider(new FakeTenantSessionStore());
			var context = CreateContext(serviceProvider, "/api/account/status", true, "unknown-tenant");
			context.User = CreateAuthenticatedPrincipal(user.Id);

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

		var services = new ServiceCollection();
		services.AddSingleton(DbContext);
		services.AddSingleton(sessionStore);
		return services.BuildServiceProvider();
	}

	private static HttpContext CreateContext(IServiceProvider serviceProvider, string path,
		bool authorizedEndpoint, string? tenantSlug, string? cookieHeader = null)
	{
		var context = new DefaultHttpContext
		{
			RequestServices = serviceProvider
		};
		context.Request.Path = path;
		if (!string.IsNullOrWhiteSpace(tenantSlug))
		{
			context.Items[TenantAuthenticationConstants.TenantSlugItemKey] = tenantSlug;
		}
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

	private static ClaimsPrincipal CreateAuthenticatedPrincipal(int userId)
	{
		var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
		var identity = new ClaimsIdentity(claims, "Basic");
		return new ClaimsPrincipal(identity);
	}

	private sealed class FakeTenantSessionStore : ITenantSessionStore
	{
		public WebSession? Session { get; set; }
		public bool TenantActivated { get; set; } = true;
		public int TouchCalls { get; private set; }

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
			TouchCalls++;
			return Task.CompletedTask;
		}
	}

	public TestContext TestContext { get; set; }
}
