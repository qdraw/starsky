using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace starsky.foundation.platform.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum QueueBackendType
{
	InMemory = 0,
	Database = 1,
	RabbitMq = 2
}

public sealed class AppSettingsRabbitMqModel
{
	public string Host { get; set; } = "localhost";
	public int Port { get; set; } = 5672;
	public string Username { get; set; } = "guest";
	public string Password { get; set; } = "guest";
	public string VirtualHost { get; set; } = "/";
}

public sealed class AppSettingsQueueModel
{
	public QueueBackendType Default { get; set; } = QueueBackendType.InMemory;

	/// <summary>
	///     Per queue backend selection.
	///     Keys should match queue name constants used by queue implementations.
	/// </summary>
	public Dictionary<string, QueueBackendType> Queues { get; set; } =
		new(StringComparer.OrdinalIgnoreCase);

	public AppSettingsRabbitMqModel RabbitMq { get; set; } = new();

	public int DatabasePollIntervalInMilliseconds { get; set; } = 500;
}


