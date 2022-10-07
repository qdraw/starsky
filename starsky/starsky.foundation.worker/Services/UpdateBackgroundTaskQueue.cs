using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using starsky.foundation.injection;
using starsky.foundation.worker.Helpers;
using starsky.foundation.worker.Interfaces;

namespace starsky.foundation.worker.Services
{
    /// <summary>
    /// @see: https://learn.microsoft.com/en-us/dotnet/core/extensions/queue-service
    /// </summary>
    [Service(typeof(IUpdateBackgroundTaskQueue), InjectionLifetime = InjectionLifetime.Singleton)]
    public sealed class UpdateBackgroundTaskQueue : IUpdateBackgroundTaskQueue
    {
	    private readonly Channel<Func<CancellationToken, ValueTask>> _queue;

	    public UpdateBackgroundTaskQueue()
	    {
		    BoundedChannelOptions options = new(int.MaxValue)
		    {
			    FullMode = BoundedChannelFullMode.Wait
		    };
		    _queue = Channel.CreateBounded<Func<CancellationToken, ValueTask>>(options);
	    }

	    public async ValueTask QueueBackgroundWorkItemAsync(
		    Func<CancellationToken, ValueTask> workItem)
	    {
		    if (workItem is null)
		    {
			    throw new ArgumentNullException(nameof(workItem));
		    }
		    await _queue.Writer.WriteAsync(workItem);
	    }

	    public async ValueTask<Func<CancellationToken, ValueTask>> DequeueAsync(
		    CancellationToken cancellationToken)
	    {
		    var workItem =
			    await _queue.Reader.ReadAsync(cancellationToken);
		    return workItem;
	    }
    }
}
