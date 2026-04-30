using System.Threading.Tasks;
using starsky.foundation.accountmanagement.Interfaces;
using starsky.foundation.database.Models.Account;

namespace starskytest.FakeMocks;

public sealed class FakeITenantSessionStore : ITenantSessionStore
{
	public Task<WebSession?> GetValidSessionAsync(string sessionId)
	{
		return Task.FromResult<WebSession?>(new WebSession { Id = 1, UserId = 0 });
	}

	public Task<WebSession> CreateOrRefreshSessionAsync(int userId,
		string? existingSessionId = null)
	{
		return Task.FromResult(new WebSession { Id = 1, UserId = userId });
	}

	public Task<bool> ActivateTenantAsync(int webSessionId, int tenantId)
	{
		return Task.FromResult(true);
	}

	public Task<bool> DeactivateTenantAsync(int webSessionId, int tenantId)
	{
		return Task.FromResult(true);
	}

	public Task RevokeSessionAsync(int webSessionId)
	{
		return Task.CompletedTask;
	}

	public Task<bool> IsTenantActivatedAsync(int webSessionId, int tenantId)
	{
		return Task.FromResult(true);
	}

	public Task TouchSessionAsync(int webSessionId, int tenantId)
	{
		return Task.CompletedTask;
	}
}

