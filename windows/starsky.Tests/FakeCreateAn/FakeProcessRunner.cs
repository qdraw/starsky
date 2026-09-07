using Starsky.Desktop.Services;

namespace starsky.Tests.FakeCreateAn;

internal sealed class FakeProcessRunner : IProcessRunner
{
    private readonly Queue<ProcessResult> _results = new();
    private readonly Queue<ProcessResult> _elevatedResults = new();

    public List<(string FileName, string Args)> RunCalls { get; } = [];
    public List<(string FileName, string Args)> RunElevatedCalls { get; } = [];

    public FakeProcessRunner EnqueueRun(int exitCode, string output = "")
    {
        _results.Enqueue(new ProcessResult(exitCode, output));
        return this;
    }

    public FakeProcessRunner EnqueueElevated(int exitCode)
    {
        _elevatedResults.Enqueue(new ProcessResult(exitCode, string.Empty));
        return this;
    }

    public ProcessResult Run(string fileName, string args)
    {
        RunCalls.Add((fileName, args));
        return _results.TryDequeue(out var r) ? r : new ProcessResult(0, string.Empty);
    }

    public ProcessResult RunElevated(string fileName, string args)
    {
        RunElevatedCalls.Add((fileName, args));
        return _elevatedResults.TryDequeue(out var r) ? r : new ProcessResult(0, string.Empty);
    }
}
