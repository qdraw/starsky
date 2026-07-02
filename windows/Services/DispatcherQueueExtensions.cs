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
}