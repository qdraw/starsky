namespace starsky.foundation.platform.Models;

public sealed class OllamaCommandResult
{
	public bool Success { get; init; }
	public string Output { get; init; } = string.Empty;
	public string Error { get; init; } = string.Empty;
	public int ExitCode { get; init; }

	public static OllamaCommandResult Failed(string error, int exitCode = -1)
	{
		return new OllamaCommandResult
		{
			Success = false,
			Error = error,
			ExitCode = exitCode
		};
	}
}

