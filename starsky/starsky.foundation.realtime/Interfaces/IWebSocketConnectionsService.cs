using System;
using System.Threading;
using System.Threading.Tasks;
using starsky.foundation.platform.Models;
using starsky.foundation.realtime.Helpers;

namespace starsky.foundation.realtime.Interfaces
{
	public interface IWebSocketConnectionsService
	{
		void AddConnection(WebSocketConnection connection);

		void RemoveConnection(Guid connectionId);

		/// <summary>
		///     To the connections of the tenant of the current request or job (TenantAmbient).
		///     Never to other tenants
		/// </summary>
		Task SendToAllAsync(string message, CancellationToken cancellationToken);

		/// <inheritdoc cref="SendToAllAsync(string,System.Threading.CancellationToken)" />
		Task SendToAllAsync<T>(ApiNotificationResponseModel<T> message, CancellationToken cancellationToken);

		/// <summary>
		///     To the connections of one tenant
		/// </summary>
		Task SendToTenantAsync(string tenantId, string message, CancellationToken cancellationToken);

		/// <summary>
		///     Close connections because the access ended: a membership that was removed, a
		///     tenant that is disabled, a user that is deleted. A connection is authorised when
		///     it is opened; this is how a revoke reaches a socket that is already open
		/// </summary>
		/// <param name="tenantId">null: of every tenant</param>
		/// <param name="userId">null: of every user</param>
		/// <param name="reason">sent to the client</param>
		Task CloseConnectionsAsync(string? tenantId, int? userId, string reason);

		/// <summary>
		///     To every connection of every tenant: only for messages without tenant data,
		///     like the heartbeat
		/// </summary>
		Task SendToEveryTenantAsync<T>(ApiNotificationResponseModel<T> message,
			CancellationToken cancellationToken);
	}
}
