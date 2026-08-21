using Microsoft.UI.Dispatching;

namespace Starsky.Windows.Services;

public static class DispatcherQueueExtensions
{
    public static Task EnqueueAsync(this DispatcherQueue dispatcherQueue, Action action)
    {
        var completion = new TaskCompletionSource();
        dispatcherQueue.TryEnqueue(() =>
        {
            try
            {
                action();
                completion.SetResult();
            }
            catch (Exception exception)
            {
                completion.SetException(exception);
            }
        });
        return completion.Task;
    }

    public static Task<T> EnqueueAsync<T>(this DispatcherQueue dispatcherQueue, Func<T> action)
    {
        var completion = new TaskCompletionSource<T>();
        dispatcherQueue.TryEnqueue(() =>
        {
            try
            {
                completion.SetResult(action());
            }
            catch (Exception exception)
            {
                completion.SetException(exception);
            }
        });
        return completion.Task;
    }

    public static Task EnqueueAsync(this DispatcherQueue dispatcherQueue, Func<Task> action)
    {
        var completion = new TaskCompletionSource();
        dispatcherQueue.TryEnqueue(async () =>
        {
            try
            {
                await action();
                completion.SetResult();
            }
            catch (Exception exception)
            {
                completion.SetException(exception);
            }
        });
        return completion.Task;
    }
}
