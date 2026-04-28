using System.Threading;
using System.Threading.Tasks;
using starsky.foundation.platform.Models;

namespace starsky.foundation.platform.Interfaces;

public interface IOllamaService
{
	Task<bool> EnsureServeIsRunning(CancellationToken cancellationToken = default);
	Task<bool> StopServeAsync(CancellationToken cancellationToken = default);
	Task<OllamaCommandResult> GenerateAsync(string prompt,
		CancellationToken cancellationToken = default);

	Task<OllamaCommandResult> InferTagsAsync(string imageFilePath,
		CancellationToken cancellationToken = default);
}

