using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using starsky.foundation.platform.JsonConverter;
using starsky.foundation.platform.Models;
using starsky.foundation.realtime.Helpers;
using starsky.foundation.realtime.Interfaces;

namespace starskytest.FakeMocks;

public class FakeIWebSocketConnectionsService : IWebSocketConnectionsService
{
	private readonly ConcurrentDictionary<Guid, WebSocketConnection> _connections = new();
	private readonly Exception? _exception;

	public FakeIWebSocketConnectionsService(Exception? exception = null)
	{
		_exception = exception;
	}

	public List<string> FakeSendToAllAsync { get; set; } = new();

	/// <summary>
	///     What went to one tenant (tenant id, message)
	/// </summary>
	public List<(string TenantId, string Message)> FakeSendToTenant { get; } = new();

	/// <summary>
	///     Add a connection to the service. This is used to simulate clients connecting to the WebSocket
	///     service.
	/// </summary>
	/// <param name="connection"></param>
	public void AddConnection(WebSocketConnection connection)
	{
		_connections.TryAdd(connection.Id, connection);
	}

	public void RemoveConnection(Guid connectionId)
	{
		throw new NotImplementedException();
	}

	public Task SendToAllAsync(string message, CancellationToken cancellationToken)
	{
		if ( _exception != null )
		{
			throw _exception;
		}

		FakeSendToAllAsync.Add(message);
		return Task.CompletedTask;
	}

	public Task SendToAllAsync<T>(ApiNotificationResponseModel<T> message,
		CancellationToken cancellationToken)
	{
		if ( _exception != null )
		{
			throw _exception;
		}

		var stringMessage = JsonSerializer.Serialize(message,
			DefaultJsonSerializer.CamelCaseNoEnters);
		FakeSendToAllAsync.Add(stringMessage);
		return Task.CompletedTask;
	}

	public Task SendToTenantAsync(string tenantId, string message,
		CancellationToken cancellationToken)
	{
		FakeSendToTenant.Add(( tenantId, message ));
		return Task.CompletedTask;
	}

	/// <summary>
	///     What was closed (tenant id, user id, reason)
	/// </summary>
	public List<(string? TenantId, int? UserId, string Reason)> FakeClosed { get; } = new();

	public Task CloseConnectionsAsync(string? tenantId, int? userId, string reason)
	{
		FakeClosed.Add(( tenantId, userId, reason ));
		return Task.CompletedTask;
	}

	public Task SendToEveryTenantAsync<T>(ApiNotificationResponseModel<T> message,
		CancellationToken cancellationToken)
	{
		return SendToAllAsync(message, cancellationToken);
	}
}
