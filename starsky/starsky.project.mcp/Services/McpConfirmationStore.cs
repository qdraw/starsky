using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using starsky.foundation.injection;

namespace starsky.project.mcp.Services;

/// <summary>
///     Short-lived, single-use tokens that tie the execute call of a write tool
///     to the preview that was shown before: same caller, same tool, same arguments
/// </summary>
[Service(typeof(McpConfirmationStore), InjectionLifetime = InjectionLifetime.Singleton)]
public sealed class McpConfirmationStore
{
	public static readonly TimeSpan Lifetime = TimeSpan.FromMinutes(5);

	private readonly ConcurrentDictionary<string, (string Fingerprint, DateTimeOffset Expires)>
		_tokens = new();

	private readonly TimeProvider _timeProvider;

	public McpConfirmationStore() : this(TimeProvider.System)
	{
	}

	public McpConfirmationStore(TimeProvider timeProvider)
	{
		_timeProvider = timeProvider;
	}

	public string Issue(string caller, string tool, IEnumerable<string> paths, string args)
	{
		RemoveExpired();
		var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(16));
		_tokens[token] = (Fingerprint(caller, tool, paths, args),
			_timeProvider.GetUtcNow().Add(Lifetime));
		return token;
	}

	/// <summary>
	///     Removes the token, so it works only once, also when the check fails
	/// </summary>
	public bool Consume(string token, string caller, string tool, IEnumerable<string> paths,
		string args)
	{
		if ( !_tokens.TryRemove(token, out var entry) )
		{
			return false;
		}

		return entry.Expires > _timeProvider.GetUtcNow() &&
		       entry.Fingerprint == Fingerprint(caller, tool, paths, args);
	}

	private void RemoveExpired()
	{
		var now = _timeProvider.GetUtcNow();
		foreach ( var expired in _tokens.Where(p => p.Value.Expires <= now) )
		{
			_tokens.TryRemove(expired.Key, out _);
		}
	}

	private static string Fingerprint(string caller, string tool, IEnumerable<string> paths,
		string args)
	{
		var text = string.Join('\n', caller, tool, args,
			string.Join('\n', paths.Order(StringComparer.Ordinal)));
		return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text)));
	}
}
