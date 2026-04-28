using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using starsky.foundation.platform.Models;

namespace starsky.foundation.platform.Interfaces;

public interface IOllamaProcessRunner
{
	bool IsServeRunning { get; }

	Task<bool> StartServeAsync(string fileName,
		IDictionary<string, string>? environmentVariables = null,
		CancellationToken cancellationToken = default);

	Task<bool> StopServeAsync(CancellationToken cancellationToken = default);

	Task<OllamaCommandResult> RunProcessWithOutputAsync(string fileName,
		string arguments,
		IDictionary<string, string>? environmentVariables = null,
		int[]? allowedExitCodes = null,
		CancellationToken cancellationToken = default);
}

