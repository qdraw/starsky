using System;
using System.Threading;
using Microsoft.Extensions.Hosting;

namespace starskytest.FakeMocks;

// Test double that allows tests to trigger the ApplicationStarted token
public class FakeTriggerableIHostApplicationLifetime : IHostApplicationLifetime, IDisposable
{
    private readonly CancellationTokenSource _startedCts = new();
    private readonly CancellationTokenSource _stoppingCts = new();
    private readonly CancellationTokenSource _stoppedCts = new();
    private bool _disposed;

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
        if ( !_startedCts.IsCancellationRequested )
        {
            _startedCts.Cancel();
        }
    }

    // Optional helpers to trigger other lifecycle events if needed
    public void TriggerApplicationStopping()
    {
        if ( !_stoppingCts.IsCancellationRequested )
        {
            _stoppingCts.Cancel();
        }
    }

    public void TriggerApplicationStopped()
    {
        if ( !_stoppedCts.IsCancellationRequested )
        {
            _stoppedCts.Cancel();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            // dispose managed
            _startedCts.Dispose();
            _stoppingCts.Dispose();
            _stoppedCts.Dispose();
        }

        // no unmanaged resources to free
        _disposed = true;
    }

    ~FakeTriggerableIHostApplicationLifetime()
    {
        Dispose(false);
    }
}

