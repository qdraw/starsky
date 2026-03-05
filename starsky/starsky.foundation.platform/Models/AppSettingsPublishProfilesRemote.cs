using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using starsky.foundation.platform.Models.PublishProfileRemote;

namespace starsky.foundation.platform.Models;

/// <summary>
///     Represents the remote Publish profiles configuration, allowing multiple remote targets per
///     profile ID.
/// </summary>
public class AppSettingsPublishProfilesRemote
{
	/// <summary>
	///     Key: publish profile id, Value: list of remote credential wrappers (targets)
	/// </summary>
	public Dictionary<string, List<RemoteCredentialWrapper>> Profiles { get; set; } = new();

	/// <summary>
	///     Default/fallback remote credentials if a profile id is not found.
	/// </summary>
	public List<RemoteCredentialWrapper> Default { get; set; } = [];

	public List<RemoteCredentialWrapper> GetById(string id,
		RemoteCredentialType? type = null)
	{
		Profiles.TryGetValue(id, out var result);
		if ( type == null )
		{
			if ( result == null || result.Count == 0 )
			{
				return Default;
			}

			return result;
		}

		if ( result != null && result.Any(p => p.Type == type) )
		{
			return result.Where(p => p.Type == type).ToList();
		}

		return result ?? Default.Where(p => p.Type == type).ToList();
	}

	public List<FtpCredential> GetFtpById(string id)
	{
		return GetById(id, RemoteCredentialType.Ftp).Where(p => p.Ftp != null).Select(p => p.Ftp)
			.Cast<FtpCredential>().ToList();
	}

	public List<LocalFileSystemCredential> GetLocalFileSystemById(string id)
	{
		return GetById(id, RemoteCredentialType.LocalFileSystem)
			.Where(p => p.LocalFileSystem != null)
			.Select(p => p.LocalFileSystem)
			.Cast<LocalFileSystemCredential>()
			.ToList();
	}

	public AppSettingsPublishProfilesRemote DisplaySecurity(string securityWarning)
	{
		foreach ( var wrapper in Profiles.SelectMany(remoteProfile => remoteProfile.Value) )
		{
			wrapper.Ftp?.SetWarning(securityWarning);
			wrapper.LocalFileSystem?.SetWarning(securityWarning);
		}

		foreach ( var wrapper in Default )
		{
			wrapper.Ftp?.SetWarning(securityWarning);
			wrapper.LocalFileSystem?.SetWarning(securityWarning);
		}

		return this;
	}

	public List<(RemoteCredentialType, string)> ListAll()
	{
		var lists = new List<(RemoteCredentialType, string)>();

		foreach ( var webFtp in Default
			         .Where(p => p.Ftp?.WebFtp != null)
			         .Select(p => p.Ftp?.WebFtp) )
		{
			lists.Add(( RemoteCredentialType.Ftp, webFtp! ));
		}

		foreach ( var webFtp in
		         Profiles
			         .SelectMany(item =>
				         item.Value)
			         .Where(p => p.Ftp?.WebFtp != null).Select(p => p.Ftp?.WebFtp) )
		{
			lists.Add(( RemoteCredentialType.Ftp, webFtp! ));
		}

		foreach ( var localFs in Default
			         .Where(p => p.LocalFileSystem?.Path != null)
			         .Select(p => p.LocalFileSystem?.Path) )
		{
			lists.Add(( RemoteCredentialType.LocalFileSystem, localFs! ));
		}

		foreach ( var localFs in
		         Profiles
			         .SelectMany(item =>
				         item.Value)
			         .Where(p => p.LocalFileSystem?.Path != null)
			         .Select(p => p.LocalFileSystem?.Path) )
		{
			lists.Add(( RemoteCredentialType.LocalFileSystem, localFs! ));
		}

		return lists;
	}
}

/// <summary>
///     Wrapper for a remote credential target (e.g., FTP, S3, etc.).
/// </summary>
public class RemoteCredentialWrapper
{
	/// <summary>
	///     The type of remote target (e.g., "ftp").
	/// </summary>
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public RemoteCredentialType Type { get; set; } = RemoteCredentialType.Ftp;

	/// <summary>
	///     FTP credential details (if applicable).
	/// </summary>
	public FtpCredential? Ftp { get; set; }

	/// <summary>
	///     Local file system credential details (if applicable).
	/// </summary>
	public LocalFileSystemCredential? LocalFileSystem { get; set; }
}

/// <summary>
///     Local file system credential details for a publish target.
/// </summary>
public class LocalFileSystemCredential
{
	private string? _path;

	/// <summary>
	///     Gets or sets the destination path on the local file system.
	/// </summary>
	public string Path
	{
		get => string.IsNullOrEmpty(_path) ? string.Empty : _path;
		set
		{
			if ( string.IsNullOrEmpty(value) )
			{
				return;
			}

			_path = value;
		}
	}

	public void SetWarning(string securityWarning)
	{
		_path = securityWarning;
	}
}
