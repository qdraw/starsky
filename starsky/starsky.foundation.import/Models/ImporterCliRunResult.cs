namespace starsky.foundation.import.Models;

public class ImporterCliRunResult
{
	public bool Success { get; set; }
	public int ExitCode { get; set; }
	public string ExecutablePath { get; set; } = string.Empty;
	public string StandardOutput { get; set; } = string.Empty;
	public string StandardError { get; set; } = string.Empty;
	public string Message { get; set; } = string.Empty;
}

