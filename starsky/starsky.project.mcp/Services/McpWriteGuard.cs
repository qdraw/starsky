using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using starsky.foundation.injection;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;

namespace starsky.project.mcp.Services;

public enum McpGateDecision
{
	/// <summary>Nothing is changed: show the preview and issue a token</summary>
	NeedsConfirmation,

	/// <summary>The token matches the preview: run the change</summary>
	Confirmed,

	/// <summary>The call is refused, see the message</summary>
	Rejected
}

public sealed record McpGateResult(
	McpGateDecision Decision,
	IReadOnlyList<string> Paths,
	string? ConfirmToken = null,
	string? Message = null);

/// <summary>
///     Guard rails in front of every MCP tool that changes files, so a tool can't skip them
/// </summary>
[Service(typeof(McpWriteGuard), InjectionLifetime = InjectionLifetime.Scoped)]
public sealed class McpWriteGuard
{
	private readonly AppSettings _appSettings;
	private readonly IHttpContextAccessor _httpContextAccessor;
	private readonly IWebLogger _logger;
	private readonly McpConfirmationStore _store;

	public McpWriteGuard(AppSettings appSettings, McpConfirmationStore store,
		IHttpContextAccessor httpContextAccessor, IWebLogger logger)
	{
		_appSettings = appSettings;
		_store = store;
		_httpContextAccessor = httpContextAccessor;
		_logger = logger;
	}

	/// <summary>
	///     The account id (the NameIdentifier claim, as used by the rest of the app) for the audit log
	/// </summary>
	private string Caller
	{
		get
		{
			var user = _httpContextAccessor.HttpContext?.User;
			return user?.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
			       user?.Identity?.Name ?? "unknown";
		}
	}

	/// <summary>
	///     Check the paths and the confirmation of one call of a write tool
	/// </summary>
	/// <param name="tool">name of the tool</param>
	/// <param name="inputPaths">subPaths from the agent</param>
	/// <param name="args">all other arguments, in a stable text format</param>
	/// <param name="confirmToken">token from the preview, or null for the first call</param>
	public McpGateResult Gate(string tool, IReadOnlyCollection<string>? inputPaths, string args,
		string? confirmToken)
	{
		if ( _appSettings.McpAllowWrite != true )
		{
			return Reject(tool, [], "Write tools are disabled (McpAllowWrite)");
		}

		var paths = new List<string>();
		foreach ( var inputPath in inputPaths ?? [] )
		{
			var path = NormalizePath(inputPath);
			if ( path == null )
			{
				return Reject(tool, [], $"Path is not allowed: {inputPath}");
			}

			if ( _appSettings.IsReadOnly(path) )
			{
				return Reject(tool, [], $"Path is read-only: {path}");
			}

			if ( !paths.Contains(path) )
			{
				paths.Add(path);
			}
		}

		if ( paths.Count == 0 )
		{
			return Reject(tool, [], "No paths given");
		}

		var max = Math.Max(1, _appSettings.McpMaxItemsPerCall);
		if ( paths.Count > max )
		{
			return Reject(tool, paths,
				$"Too many paths: {paths.Count}, the maximum is {max} per call");
		}

		if ( string.IsNullOrWhiteSpace(confirmToken) )
		{
			var token = _store.Issue(Caller, tool, paths, args);
			_logger.LogInformation(
				$"[Mcp] user={Caller} tool={tool} phase=preview paths={string.Join(';', paths)}");
			return new McpGateResult(McpGateDecision.NeedsConfirmation, paths, token);
		}

		if ( !_store.Consume(confirmToken, Caller, tool, paths, args) )
		{
			return Reject(tool, paths,
				"confirmToken is unknown, expired, already used or for different arguments. " +
				"Call again without a token to get a new preview");
		}

		_logger.LogInformation(
			$"[Mcp] user={Caller} tool={tool} phase=execute paths={string.Join(';', paths)}");
		return new McpGateResult(McpGateDecision.Confirmed, paths);
	}

	/// <summary>
	///     Write the result of an executed change to the log
	/// </summary>
	public void Audit(string tool, IEnumerable<string> paths, string result)
	{
		_logger.LogInformation(
			$"[Mcp] user={Caller} tool={tool} phase=done result={result} paths={string.Join(';', paths)}");
	}

	/// <summary>
	///     Only a subPath inside the storage folder: no traversal, no root, no control characters
	/// </summary>
	internal static string? NormalizePath(string? input)
	{
		if ( string.IsNullOrWhiteSpace(input) || input.Any(char.IsControl) ||
		     input.Contains('\\') || input.Contains(';') )
		{
			return null;
		}

		var path = PathHelper.RemoveLatestSlash(PathHelper.PrefixDbSlash(input.Trim()));
		if ( string.IsNullOrEmpty(path) || path == "/" ||
		     path.Split('/').Contains("..") )
		{
			return null;
		}

		return path;
	}

	/// <summary>
	///     Same checks as for the input paths, for a path that is not part of the confirmation
	///     paths (the new name when renaming). Null when not allowed
	/// </summary>
	public string? ValidateTarget(string? input)
	{
		var path = NormalizePath(input);
		return path == null || _appSettings.IsReadOnly(path) ? null : path;
	}

	private McpGateResult Reject(string tool, IReadOnlyList<string> paths, string message)
	{
		_logger.LogInformation(
			$"[Mcp] user={Caller} tool={tool} phase=rejected reason={message}");
		return new McpGateResult(McpGateDecision.Rejected, paths, null, message);
	}
}
