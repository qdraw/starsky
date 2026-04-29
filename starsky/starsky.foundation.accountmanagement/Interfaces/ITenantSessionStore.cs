using System.Threading.Tasks;
using starsky.foundation.database.Models.Account;

namespace starsky.foundation.accountmanagement.Interfaces;

public interface ITenantSessionStore
{
	Task<WebSession?> GetValidSessionAsync(string sessionId);
	Task<WebSession> CreateOrRefreshSessionAsync(int userId, string? existingSessionId = null);
	Task<bool> ActivateTenantAsync(int webSessionId, int tenantId);
	Task<bool> DeactivateTenantAsync(int webSessionId, int tenantId);
	Task RevokeSessionAsync(int webSessionId);
	Task<bool> IsTenantActivatedAsync(int webSessionId, int tenantId);
	Task TouchSessionAsync(int webSessionId, int tenantId);
}
