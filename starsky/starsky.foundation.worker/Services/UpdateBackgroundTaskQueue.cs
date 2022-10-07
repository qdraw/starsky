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
	    private readonly Channel<Tuple<Func<CancellationToken, ValueTask>, string>> _queue;

	    public UpdateBackgroundTaskQueue()
	    {
		    BoundedChannelOptions options = new(int.MaxValue)
		    {
			    FullMode = BoundedChannelFullMode.Wait
		    };
		    _queue = Channel.CreateBounded<Tuple<Func<CancellationToken, ValueTask>, string>>(options);
	    }
	    
	    public int Count()
	    {
		    return _queue.Reader.Count;
	    }
	    
	    public ValueTask QueueBackgroundWorkItemAsync(
		    Func<CancellationToken, ValueTask> workItem, string metaData)
	    {
		    if (workItem is null)
		    {
			    throw new ArgumentNullException(nameof(workItem));
		    }

		    return QueueBackgroundWorkItemInternalAsync(workItem, metaData);
	    }

	    private async ValueTask QueueBackgroundWorkItemInternalAsync(
		    Func<CancellationToken, ValueTask> workItem, string metaData)
	    {
		    await _queue.Writer.WriteAsync(new Tuple<Func<CancellationToken, ValueTask>, string>(workItem,metaData));
	    }

	    public async ValueTask<Tuple<Func<CancellationToken, ValueTask>, string>> DequeueAsync(
		    CancellationToken cancellationToken)
	    {
		    return await _queue.Reader.ReadAsync(cancellationToken);
	    }
    }
}
