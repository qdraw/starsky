using System.Threading;
using Microsoft.Extensions.Hosting;

namespace starskytest.FakeMocks;

// Test double that allows tests to trigger the ApplicationStarted token
public class FakeTriggerableIHostApplicationLifetime : IHostApplicationLifetime
{
    private readonly CancellationTokenSource _startedCts = new();
    private readonly CancellationTokenSource _stoppingCts = new();
    private readonly CancellationTokenSource _stoppedCts = new();

    public void StopApplication()
    {
        // no-op for tests
    }

    public CancellationToken ApplicationStarted => _startedCts.Token;
    public CancellationToken ApplicationStopping => _stoppingCts.Token;
    public CancellationToken ApplicationStopped => _stoppedCts.Token;

    // Trigger the ApplicationStarted token to invoke registered callbacks
    public void TriggerApplicationStarted()
    {
        try
        {
            _startedCts.Cancel();
        }
        catch
        {
            // ignore if already canceled
        }
    }

    // Optional helpers to trigger other lifecycle events if needed
    public void TriggerApplicationStopping()
    {
        try { _stoppingCts.Cancel(); } catch { }
    }

    public void TriggerApplicationStopped()
    {
        try { _stoppedCts.Cancel(); } catch { }
    }
}

