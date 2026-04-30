using System;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using starsky.foundation.accountmanagement.Interfaces;
using starsky.foundation.database.Data;
using starsky.foundation.database.Models.Account;
using starsky.foundation.injection;

namespace starsky.foundation.accountmanagement.Services;

[Service(typeof(ITenantSessionStore), InjectionLifetime = InjectionLifetime.Scoped)]
public sealed class TenantSessionStore(ApplicationDbContext dbContext) : ITenantSessionStore
{
	private static readonly TimeSpan SessionLifetime = TimeSpan.FromDays(60);

	public async Task<WebSession?> GetValidSessionAsync(string sessionId)
	{
		if (string.IsNullOrWhiteSpace(sessionId))
		{
			return null;
		}

		return await dbContext.WebSessions
			.FirstOrDefaultAsync(s =>
				s.SessionId == sessionId &&
				s.RevokedAt == null &&
				s.ExpiresAt > DateTime.UtcNow);
	}

	public async Task<WebSession> CreateOrRefreshSessionAsync(int userId, string? existingSessionId = null)
	{
		WebSession? session = null;
		if (!string.IsNullOrWhiteSpace(existingSessionId))
		{
			session = await GetValidSessionAsync(existingSessionId);
		}

		if (session == null)
		{
			session = new WebSession
			{
				UserId = userId,
				SessionId = GenerateOpaqueSessionId(),
				Created = DateTime.UtcNow,
				LastSeen = DateTime.UtcNow,
				ExpiresAt = DateTime.UtcNow.Add(SessionLifetime)
			};
			await dbContext.WebSessions.AddAsync(session);
		}
		else
		{
			session.UserId = userId;
			session.LastSeen = DateTime.UtcNow;
			session.ExpiresAt = DateTime.UtcNow.Add(SessionLifetime);
			dbContext.WebSessions.Update(session);
		}

		await dbContext.SaveChangesAsync();
		return session;
	}

	public async Task<bool> ActivateTenantAsync(int webSessionId, int tenantId)
	{
		var item = await dbContext.WebSessionTenants
			.FirstOrDefaultAsync(w => w.WebSessionId == webSessionId && w.TenantId == tenantId);

		if (item == null)
		{
			await dbContext.WebSessionTenants.AddAsync(new WebSessionTenant
			{
				WebSessionId = webSessionId,
				TenantId = tenantId,
				Created = DateTime.UtcNow,
				LastSeen = DateTime.UtcNow
			});
		}
		else
		{
			item.LastSeen = DateTime.UtcNow;
			dbContext.WebSessionTenants.Update(item);
		}

		await dbContext.SaveChangesAsync();
		return true;
	}

	public async Task<bool> DeactivateTenantAsync(int webSessionId, int tenantId)
	{
		var item = await dbContext.WebSessionTenants
			.FirstOrDefaultAsync(w => w.WebSessionId == webSessionId && w.TenantId == tenantId);
		if (item == null)
		{
			return false;
		}

		dbContext.WebSessionTenants.Remove(item);
		await dbContext.SaveChangesAsync();
		return true;
	}

	public async Task RevokeSessionAsync(int webSessionId)
	{
		var session = await dbContext.WebSessions.FirstOrDefaultAsync(s => s.Id == webSessionId);
		if (session == null)
		{
			return;
		}

		session.RevokedAt = DateTime.UtcNow;
		dbContext.WebSessions.Update(session);
		await dbContext.SaveChangesAsync();
	}

	public async Task<bool> IsTenantActivatedAsync(int webSessionId, int tenantId)
	{
		return await dbContext.WebSessionTenants
			.AnyAsync(w => w.WebSessionId == webSessionId && w.TenantId == tenantId);
	}

	public async Task TouchSessionAsync(int webSessionId, int tenantId)
	{
		var session = await dbContext.WebSessions.FirstOrDefaultAsync(s => s.Id == webSessionId);
		if (session != null)
		{
			session.LastSeen = DateTime.UtcNow;
			dbContext.WebSessions.Update(session);
		}

		var tenantSession = await dbContext.WebSessionTenants
			.FirstOrDefaultAsync(w => w.WebSessionId == webSessionId && w.TenantId == tenantId);
		if (tenantSession != null)
		{
			tenantSession.LastSeen = DateTime.UtcNow;
			dbContext.WebSessionTenants.Update(tenantSession);
		}

		await dbContext.SaveChangesAsync();
	}

	private static string GenerateOpaqueSessionId()
	{
		var bytes = RandomNumberGenerator.GetBytes(48);
		return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
	}
}
