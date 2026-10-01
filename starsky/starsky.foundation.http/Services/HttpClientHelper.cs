using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.http.Interfaces;
using starsky.foundation.injection;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;

[assembly: InternalsVisibleTo("starskytest")]

namespace starsky.foundation.http.Services;

[Service(typeof(IHttpClientHelper), InjectionLifetime = InjectionLifetime.Singleton)]
public sealed class HttpClientHelper : IHttpClientHelper
{
	/// <summary>
	///     These domains are only allowed domains to download from (and https only)
	/// </summary>
	private readonly List<string> _allowedDomains =
	[
		"qdraw.nl", // < used by test and dependencies
		"media.qdraw.nl", // < used by demo
		"locker.ifttt.com",
		"download.geonames.org",
		"exiftool.org",
		"sourceforge.net",
		"api.github.com",
		"starsky-dependencies.netlify.app",
		"api.dropbox.com",
		"nominatim.openstreetmap.org"
	];

	/// <summary>
	///     Max size of a restricted (user supplied url) download, matches the upload limit
	/// </summary>
	internal const long MaxRestrictedDownloadBytes = 320L * 1024 * 1024;

	internal const int MaxRedirectHops = 3;

	private readonly long _maxRestrictedDownloadBytes = MaxRestrictedDownloadBytes;

	/// <summary>
	///     Http Provider
	/// </summary>
	private readonly IHttpProvider _httpProvider;

	private readonly IWebLogger _logger;
	private readonly IStorage? _storage;

	internal HttpClientHelper(IHttpProvider httpProvider,
		IStorage? storage, IWebLogger logger, long? maxRestrictedDownloadBytes = null)
	{
		_maxRestrictedDownloadBytes = maxRestrictedDownloadBytes ?? MaxRestrictedDownloadBytes;
		_httpProvider = httpProvider;
		_logger = logger;
		_storage = storage;
	}

	/// <summary>
	///     Set Http Provider
	/// </summary>
	/// <param name="httpProvider">IHttpProvider</param>
	/// <param name="serviceScopeFactory">ScopeFactory contains a IStorageSelector</param>
	/// <param name="logger">WebLogger</param>
	public HttpClientHelper(IHttpProvider httpProvider,
		IServiceScopeFactory? serviceScopeFactory, IWebLogger logger)
	{
		_httpProvider = httpProvider;
		_logger = logger;
		if ( serviceScopeFactory == null )
		{
			return;
		}

		using ( var scope = serviceScopeFactory.CreateScope() )
		{
			// ISelectorStorage is a scoped service
			var selectorStorage = scope.ServiceProvider.GetRequiredService<ISelectorStorage>();
			_storage = selectorStorage.Get(SelectorStorage.StorageServices.HostFilesystem);
		}
	}

	public async Task<KeyValuePair<bool, string>> ReadString(string sourceHttpUrl)
	{
		var sourceUri = new Uri(sourceHttpUrl);
		return await ReadString(sourceUri);
	}

	public async Task<KeyValuePair<bool, string>> ReadString(Uri sourceHttpUrl)
	{
		_logger.LogInformation("[ReadString] HttpClientHelper > "
		                       + sourceHttpUrl);
		// allow whitelist and https only
		if ( !_allowedDomains.Contains(sourceHttpUrl.Host) || sourceHttpUrl.Scheme != "https" )
		{
			return
				new KeyValuePair<bool, string>(false, string.Empty);
		}

		try
		{
			using ( var response = await _httpProvider.GetAsync(sourceHttpUrl.ToString()) )
			{
				if ( response?.Content == null )
				{
					return new KeyValuePair<bool, string>(false, string.Empty);
				}

				using ( var streamToReadFrom = await response.Content.ReadAsStreamAsync() )
				{
					var reader = new StreamReader(streamToReadFrom, Encoding.UTF8);
					var result = await reader.ReadToEndAsync();
					return new KeyValuePair<bool, string>(response.StatusCode == HttpStatusCode.OK,
						result);
				}
			}
		}
		catch ( Exception exception )
		{
			_logger.LogError(
				$"[ReadString] HttpClientHelper {sourceHttpUrl} > Exception {exception.Message}",
				exception);
			return new KeyValuePair<bool, string>(false, exception.Message);
		}
	}

	public async Task<KeyValuePair<bool, string>> PostString(string sourceHttpUrl,
		HttpContent? httpContent, AuthenticationHeaderValue? authenticationHeaderValue = null,
		bool verbose = true)
	{
		var sourceUri = new Uri(sourceHttpUrl);

		if ( verbose )
		{
			_logger.LogInformation("[PostString] HttpClientHelper > "
			                       + sourceUri.Host + " ~ " + sourceHttpUrl);
		}

		// // allow whitelist and https only
		if ( !_allowedDomains.Contains(sourceUri.Host) || sourceUri.Scheme != "https" )
		{
			return
				new KeyValuePair<bool, string>(false, string.Empty);
		}

		try
		{
			using ( var response = await _httpProvider.PostAsync(sourceHttpUrl,
				       httpContent,
				       authenticationHeaderValue) )
			using ( var streamToReadFrom = await response.Content.ReadAsStreamAsync() )
			{
				var reader = new StreamReader(streamToReadFrom, Encoding.UTF8);
				var result = await reader.ReadToEndAsync();
				return new KeyValuePair<bool, string>(response.StatusCode == HttpStatusCode.OK,
					result);
			}
		}
		catch ( Exception exception )
		{
			_logger.LogError($"[PostString] HttpClientHelper {sourceHttpUrl} > Exception ",
				exception);
			return new KeyValuePair<bool, string>(false, exception.Message);
		}
	}

	public async Task<bool> Download(string sourceHttpUrl, string fullLocalPath,
		int retryAfterInSeconds = 15, string? userAgent = null, bool restricted = false)
	{
		if ( !Uri.TryCreate(sourceHttpUrl, UriKind.Absolute, out var sourceUri) )
		{
			_logger.LogInformation("[Download] HttpClientHelper > skip: invalid url");
			return false;
		}

		return await Download(sourceUri, fullLocalPath, retryAfterInSeconds, userAgent,
			restricted);
	}

	/// <summary>
	///     Downloads the specified source HTTPS URL.
	/// </summary>
	/// <param name="sourceUri">The source HTTPS URL.</param>
	/// <param name="fullLocalPath">The full local path.</param>
	/// <param name="retryAfterInSeconds">Retry after number of seconds</param>
	/// <param name="userAgent">Optional user-agent override</param>
	/// <param name="restricted">no auto redirects (allowlist per hop) and size limit</param>
	/// <returns></returns>
	public async Task<bool> Download(Uri sourceUri, string fullLocalPath,
		int retryAfterInSeconds = 15, string? userAgent = null, bool restricted = false)
	{
		if ( _storage == null )
		{
			throw new EndOfStreamException("is null " + nameof(_storage));
		}

		_logger.LogInformation("[Download] HttpClientHelper > "
		                       + " ~ " + sourceUri);

		// allow whitelist and https only
		if ( !_allowedDomains.Contains(sourceUri.Host) ||
		     sourceUri.Scheme != "https" )
		{
			_logger.LogInformation("[Download] HttpClientHelper > "
			                       + "skip: domain not whitelisted " + " ~ " + sourceUri);
			return false;
		}

		async Task<bool> DownloadAsync()
		{
			using var response = restricted
				? await GetFollowingAllowedRedirects(sourceUri, userAgent)
				: await _httpProvider.GetAsync(sourceUri.ToString(), userAgent);
			if ( response == null )
			{
				return false;
			}

			await using var streamToReadFrom = await response.Content.ReadAsStreamAsync();
			if ( response.StatusCode != HttpStatusCode.OK )
			{
				_logger.LogInformation("[Download] HttpClientHelper > " +
				                       response.StatusCode + " ~ " + sourceUri);
				return false;
			}

			if ( !restricted )
			{
				await _storage.WriteStreamAsync(streamToReadFrom, fullLocalPath);
				return true;
			}

			// do not trust the header only, the stream is limited while copying
			if ( response.Content.Headers.ContentLength > _maxRestrictedDownloadBytes )
			{
				_logger.LogInformation("[Download] HttpClientHelper > skip: " +
				                       "content-length too large ~ " + sourceUri);
				return false;
			}

			await using var limited =
				new LimitedReadStream(streamToReadFrom, _maxRestrictedDownloadBytes);
			try
			{
				await _storage.WriteStreamAsync(limited, fullLocalPath);
			}
			catch ( DownloadTooLargeException )
			{
				_logger.LogInformation("[Download] HttpClientHelper > skip: " +
				                       "download too large ~ " + sourceUri);
				_storage.FileDelete(fullLocalPath);
				return false;
			}

			return true;
		}

		try
		{
			return await RetryHelper.DoAsync(DownloadAsync,
				TimeSpan.FromSeconds(retryAfterInSeconds), 2);
		}
		catch ( AggregateException exception )
		{
			foreach ( var innerException in exception.InnerExceptions )
			{
				_logger.LogError(innerException, $"[Download] InnerException: {exception.Message}");
			}

			_logger.LogError(exception, $"[Download] Exception: {sourceUri} " +
			                            $"{exception.Message}");
			return false;
		}
	}

	/// <summary>
	///     Follow redirects manually so the allowlist (https + domain) is checked on every hop
	/// </summary>
	/// <returns>the final response or null when a hop is not allowed or too many hops</returns>
	private async Task<HttpResponseMessage?> GetFollowingAllowedRedirects(Uri sourceUri,
		string? userAgent)
	{
		var current = sourceUri;
		for ( var hop = 0; hop <= MaxRedirectHops; hop++ )
		{
			var response = await _httpProvider.GetAsync(current.ToString(), userAgent, false);
			var status = ( int ) response.StatusCode;
			if ( status is < 300 or >= 400 )
			{
				return response;
			}

			var location = response.Headers.Location;
			response.Dispose();
			if ( location == null )
			{
				return null;
			}

			var next = location.IsAbsoluteUri ? location : new Uri(current, location);
			if ( !_allowedDomains.Contains(next.Host) || next.Scheme != "https" )
			{
				_logger.LogInformation("[Download] HttpClientHelper > " +
				                       "skip: redirect target not whitelisted ~ " + next.Host);
				return null;
			}

			current = next;
		}

		_logger.LogInformation("[Download] HttpClientHelper > skip: too many redirects");
		return null;
	}

	private sealed class DownloadTooLargeException : IOException
	{
	}

	/// <summary>
	///     Throws when more than the limit is read
	/// </summary>
	private sealed class LimitedReadStream(Stream inner, long limit) : Stream
	{
		private long _total;

		public override bool CanRead => true;
		public override bool CanSeek => false;
		public override bool CanWrite => false;
		public override long Length => throw new NotSupportedException();

		public override long Position
		{
			get => throw new NotSupportedException();
			set => throw new NotSupportedException();
		}

		private int Track(int read)
		{
			_total += read;
			if ( _total > limit )
			{
				throw new DownloadTooLargeException();
			}

			return read;
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			return Track(inner.Read(buffer, offset, count));
		}

		public override async ValueTask<int> ReadAsync(Memory<byte> buffer,
			CancellationToken cancellationToken = default)
		{
			return Track(await inner.ReadAsync(buffer, cancellationToken));
		}

		public override async Task<int> ReadAsync(byte[] buffer, int offset, int count,
			CancellationToken cancellationToken)
		{
			return Track(await inner.ReadAsync(buffer.AsMemory(offset, count),
				cancellationToken));
		}

		public override void Flush()
		{
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotSupportedException();
		}

		public override void SetLength(long value)
		{
			throw new NotSupportedException();
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			throw new NotSupportedException();
		}
	}
}
