using System;

namespace starsky.foundation.platform.Models.PublishProfileRemote;

public sealed class FtpCredential
{
	private const string PathDelimiter = "/";
	private string _path = "/";

	/* =========================
	 *  Structured fields
	 * ========================= */

	public string Host { get; set; } = string.Empty;

	public string Path
	{
		get => _path;
		set
		{
			if ( string.IsNullOrWhiteSpace(value) )
			{
				_path = "/";
				return;
			}

			_path = value.StartsWith('/') ? value : PathDelimiter + value;
		}
	}

	public string Password { get; set; } = string.Empty;

	public string Username { get; set; } = string.Empty;

	/* =========================
	 *  URI view (two-way)
	 * ========================= */

	/// <summary>
	///     FTP URL including credentials.
	///     Credentials are percent-encoded.
	/// </summary>
	public string WebFtp
	{
		get
		{
			if ( string.IsNullOrEmpty(Host) ||
			     string.IsNullOrEmpty(Username) ||
			     string.IsNullOrEmpty(Password) )
			{
				return string.Empty;
			}

			var user = Uri.EscapeDataString(Username);

			var password = Uri.EscapeDataString(Password);
			if ( Password == AppSettings.CloneToDisplaySecurityWarning )
			{
				// do not escape the warning message, to make it more readable
				password = Password;
			}

			return $"{Scheme}://{user}:{password}@{Host}{_path}";
		}
		set
		{
			if ( string.IsNullOrWhiteSpace(value) )
			{
				return;
			}

			if ( !Uri.TryCreate(value, UriKind.Absolute, out var uri) )
			{
				return;
			}

			var isFtpScheme = string.Equals(uri.Scheme, Uri.UriSchemeFtp,
				StringComparison.OrdinalIgnoreCase);
			var isFtpsScheme = string.Equals(uri.Scheme, "ftps",
				StringComparison.OrdinalIgnoreCase);
			if ( !isFtpScheme && !isFtpsScheme )
			{
				return;
			}

			if ( string.IsNullOrEmpty(uri.UserInfo) )
			{
				return; // anonymous FTP not supported
			}

			var parts = uri.UserInfo.Split(':', 2);
			if ( parts.Length != 2 )
			{
				return;
			}

			Username = Uri.UnescapeDataString(parts[0]);
			Password = Uri.UnescapeDataString(parts[1]);
			Host = uri.Host;
			Scheme = uri.Scheme;
			_path = string.IsNullOrEmpty(uri.AbsolutePath) ? "/" : uri.AbsolutePath;
		}
	}

	public string Scheme { get; set; } = "ftp";


	/// <summary>
	///     FTP URL without credentials.
	/// </summary>
	public string WebFtpNoLogin =>
		string.IsNullOrEmpty(Host)
			? string.Empty
			: $"ftp://{Host}{_path}";

	public void SetWarning(string securityWarning)
	{
		Password = securityWarning;
	}
}
