using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.imageclassification.Interfaces;
using starsky.foundation.injection;
using starsky.foundation.worker.Backends;
using starsky.foundation.worker.Interfaces;
using starsky.foundation.worker.Models;

namespace starsky.foundation.imageclassification.Services;

[Service(typeof(IImageClassificationBackgroundTaskQueue),
	InjectionLifetime = InjectionLifetime.Singleton)]
public sealed class ImageClassificationBackgroundTaskQueue :
	IImageClassificationBackgroundTaskQueue
{
	public const string QueueName = QueueNames.ImageClassification;

	private readonly IBaseBackgroundTaskQueue _backend;

	public ImageClassificationBackgroundTaskQueue(IServiceScopeFactory scopeFactory,
		IQueueBackendFactory? queueBackendFactory = null)
	{
		_ = scopeFactory;
		_backend = queueBackendFactory?.Create(QueueName) ?? new InMemoryQueueBackend();
	}

	public int Count()
	{
		return _backend.Count();
	}

	public ValueTask QueueJobAsync(BackgroundTaskQueueJob job)
	{
		return _backend.QueueJobAsync(job);
	}

	public ValueTask<BackgroundTaskQueueJob> DequeueJobAsync(CancellationToken cancellationToken)
	{
		return _backend.DequeueJobAsync(cancellationToken);
	}
}

