using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.injection;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.worker.Backends;
using starsky.foundation.worker.Interfaces;

namespace starsky.foundation.worker.Services;

[Service(typeof(IQueueBackendFactory), InjectionLifetime = InjectionLifetime.Singleton)]
public sealed class QueueBackendFactory(
	IServiceScopeFactory scopeFactory,
	IWebLogger logger,
	AppSettings appSettings) : IQueueBackendFactory
{
	private readonly ConcurrentDictionary<string, IBaseBackgroundTaskQueue> _instances =
		new(StringComparer.OrdinalIgnoreCase);

	public IBaseBackgroundTaskQueue Create(string queueName)
	{
		if ( string.IsNullOrWhiteSpace(queueName) )
		{
			throw new ArgumentException("Queue name is required", nameof(queueName));
		}

		return _instances.GetOrAdd(queueName, _ => CreateBySettings(queueName));
	}

	private IBaseBackgroundTaskQueue CreateBySettings(string queueName)
	{
		var backendType = appSettings.Queue.Queues.TryGetValue(queueName, out var perQueue)
			? perQueue
			: appSettings.Queue.Default;

		logger.LogInformation($"[QueueBackendFactory] Queue {queueName} uses backend {backendType}");

		return backendType switch
		{
			QueueBackendType.Database =>
				new DatabaseQueueBackend(scopeFactory, appSettings, logger, queueName),
			QueueBackendType.RabbitMq =>
				new RabbitMqQueueBackend(appSettings, logger, queueName),
			_ => new InMemoryQueueBackend()
		};
	}
}


