using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

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
///     FTP credential details for a remote Publish target.
/// </summary>
public class FtpCredential
{
	private string? _webFtp;

	/// <summary>
	///     Gets or sets the FTP URL (with credentials). '@' in username should be '%40'.
	/// </summary>
	public string WebFtp
	{
		get => string.IsNullOrEmpty(_webFtp) ? string.Empty : _webFtp;
		set
		{
			// Anonymous FTP is not supported
			// Make sure that '@' in username is '%40'
			if ( string.IsNullOrEmpty(value) )
			{
				return;
			}

			try
			{
				var uriAddress = new Uri(value);
				if ( uriAddress.UserInfo.Split(":".ToCharArray()).Length == 2
				     && uriAddress is { Scheme: "ftp", LocalPath.Length: >= 1 } )
				{
					_webFtp = value;
				}
			}
			catch ( UriFormatException )
			{
				// Invalid URI format, do not set the value
			}
		}
	}

	private string[] Credentials
	{
		get
		{
			try
			{
				return new Uri(WebFtp).UserInfo.Split(":".ToCharArray());
			}
			catch ( UriFormatException )
			{
				return [];
			}
		}
	}

	public string Username => Credentials is not { Length: 2 } ? string.Empty : Credentials[0];
	public string Password => Credentials is not { Length: 2 } ? string.Empty : Credentials[1];

	/// <summary>
	///     eg ftp://service.nl/drop/
	/// </summary>
	public string WebFtpNoLogin
	{
		get
		{
			try
			{
				var uri = new Uri(WebFtp);
				return $"{uri.Scheme}://{uri.Host}{uri.LocalPath}";
			}
			catch ( UriFormatException )
			{
				return string.Empty;
			}
		}
	}

	public void SetWarning(string securityWarning)
	{
		_webFtp = securityWarning;
	}
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
