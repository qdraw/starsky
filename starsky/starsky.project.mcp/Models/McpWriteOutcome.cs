namespace starsky.project.mcp.Models;

/// <summary>
///     Result of a tool that changes files. `preview` changes nothing,
///     call again with the confirmToken to execute
/// </summary>
/// <param name="Status">preview, executed or rejected</param>
/// <param name="Message">what happened or what to do next</param>
/// <param name="ConfirmToken">only with preview: pass it to the same call to execute</param>
/// <param name="Items">the affected files</param>
public sealed record McpWriteOutcome(
	string Status,
	string Message,
	string? ConfirmToken,
	IReadOnlyList<McpPhoto> Items)
{
	public const string Preview = "preview";
	public const string Executed = "executed";
	public const string Rejected = "rejected";

	public static McpWriteOutcome Reject(string message)
	{
		return new McpWriteOutcome(Rejected, message, null, []);
	}
}
