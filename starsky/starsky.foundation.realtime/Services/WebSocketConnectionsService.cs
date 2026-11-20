using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using starsky.foundation.injection;
using starsky.foundation.platform.JsonConverter;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Tenancy;
using starsky.foundation.realtime.Helpers;
using starsky.foundation.realtime.Interfaces;
using System.Net.WebSockets;

namespace starsky.foundation.realtime.Services;

[Service(typeof(IWebSocketConnectionsService), InjectionLifetime = InjectionLifetime.Singleton)]
public sealed class WebSocketConnectionsService : IWebSocketConnectionsService
{
	private readonly ConcurrentDictionary<Guid, WebSocketConnection> _connections = new();

	public void AddConnection(WebSocketConnection connection)
	{
		_connections.TryAdd(connection.Id, connection);
	}

	public void RemoveConnection(Guid connectionId)
	{
		_connections.TryRemove(connectionId, out _);
	}

	public Task SendToAllAsync(string message, CancellationToken cancellationToken)
	{
		return SendToTenantAsync(TenantAmbient.Current, message, cancellationToken);
	}

	public Task SendToAllAsync<T>(ApiNotificationResponseModel<T> message,
		CancellationToken cancellationToken)
	{
		return SendToTenantAsync(TenantAmbient.Current, Serialize(message), cancellationToken);
	}

	public async Task SendToTenantAsync(string tenantId, string message,
		CancellationToken cancellationToken)
	{
		await Task.WhenAll(_connections.Values.Where(connection => connection.TenantId == tenantId)
			.Select(connection => connection.SendAsync(message, cancellationToken)));
	}

	public async Task CloseConnectionsAsync(string? tenantId, int? userId, string reason)
	{
		var toClose = _connections.Values.Where(connection =>
			( tenantId == null || connection.TenantId == tenantId ) &&
			( userId == null || connection.UserId == userId )).ToList();
		foreach ( var connection in toClose )
		{
			// 1008: PolicyViolation
			await connection.CloseAsync(WebSocketCloseStatus.PolicyViolation, reason);
			_connections.TryRemove(connection.Id, out _);
		}
	}

	public async Task SendToEveryTenantAsync<T>(ApiNotificationResponseModel<T> message,
		CancellationToken cancellationToken)
	{
		var stringMessage = Serialize(message);
		await Task.WhenAll(_connections.Values.Select(connection =>
			connection.SendAsync(stringMessage, cancellationToken)));
	}

	private static string Serialize<T>(ApiNotificationResponseModel<T> message)
	{
		return JsonSerializer.Serialize(message, DefaultJsonSerializer.CamelCaseNoEnters);
	}
}
