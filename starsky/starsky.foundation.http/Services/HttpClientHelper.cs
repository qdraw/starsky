#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.http.Interfaces;
using starsky.foundation.injection;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;

namespace starsky.foundation.http.Services
{
	[Service(typeof(IHttpClientHelper), InjectionLifetime = InjectionLifetime.Singleton)]
    public class HttpClientHelper : IHttpClientHelper
    {
	    private readonly IStorage _storage;

	    /// <summary>
	    /// Set Http Provider
	    /// </summary>
	    /// <param name="httpProvider">IHttpProvider</param>
	    /// <param name="serviceScopeFactory">ScopeFactory contains a IStorageSelector</param>
	    /// <param name="logger">WebLogger</param>
	    public HttpClientHelper(IHttpProvider httpProvider, IServiceScopeFactory serviceScopeFactory, IWebLogger logger)
	    {
		    _httpProvider = httpProvider;
		    _logger = logger;
		    if ( serviceScopeFactory == null )  return;

		    using ( var scope = serviceScopeFactory.CreateScope() )
		    {
			    // ISelectorStorage is a scoped service
			    var selectorStorage = scope.ServiceProvider.GetRequiredService<ISelectorStorage>();
			    _storage = selectorStorage.Get(SelectorStorage.StorageServices.HostFilesystem);
		    }
	    }

	    /// <summary>
	    /// Http Provider
	    /// </summary>
	    private readonly IHttpProvider _httpProvider;
	    
	    private readonly IWebLogger _logger;

		/// <summary>
		/// This domains are only allowed domains to download from (and https only)
		/// </summary>
		private readonly List<string> AllowedDomains = new List<string>
        {
            "dl.dropboxusercontent.com", 
            "qdraw.nl", // < used by test
            "locker.ifttt.com",
			"download.geonames.org",
			"exiftool.org",
			"api.github.com"
		};

		public async Task<KeyValuePair<bool,string>> ReadString(string sourceHttpUrl)
		{
			Uri sourceUri = new Uri(sourceHttpUrl);

			_logger.LogInformation("[ReadString] HttpClientHelper > " + sourceUri.Host + " ~ " + sourceHttpUrl);

			// allow whitelist and https only
			if (!AllowedDomains.Contains(sourceUri.Host) || sourceUri.Scheme != "https") return 
				new KeyValuePair<bool, string>(false,string.Empty);

			try
			{
				using (HttpResponseMessage response = await _httpProvider.GetAsync(sourceHttpUrl))
				using (Stream streamToReadFrom = await response.Content.ReadAsStreamAsync())
				{
					var reader = new StreamReader(streamToReadFrom, Encoding.UTF8);
					var result = await reader.ReadToEndAsync();
					return new KeyValuePair<bool, string>(response.StatusCode == HttpStatusCode.OK,result);
				}
			}
			catch (HttpRequestException exception)
			{
				return new KeyValuePair<bool, string>(false, exception.Message);
			}
		}
		
		public async Task<KeyValuePair<bool,string>> PostString(string sourceHttpUrl, HttpContent? httpContent, bool verbose = true)
		{
			Uri sourceUri = new Uri(sourceHttpUrl);

			if ( verbose ) _logger.LogInformation("[PostString] HttpClientHelper > " + sourceUri.Host + " ~ " + sourceHttpUrl);

			// // allow whitelist and https only
			if (!AllowedDomains.Contains(sourceUri.Host) || sourceUri.Scheme != "https") return 
				new KeyValuePair<bool, string>(false,string.Empty);

			try
			{
				using (HttpResponseMessage response = await _httpProvider.PostAsync(sourceHttpUrl, httpContent))
				using (Stream streamToReadFrom = await response.Content.ReadAsStreamAsync())
				{
					var reader = new StreamReader(streamToReadFrom, Encoding.UTF8);
					var result = await reader.ReadToEndAsync();
					return new KeyValuePair<bool, string>(response.StatusCode == HttpStatusCode.OK,result);
				}
			}
			catch (HttpRequestException exception)
			{
				return new KeyValuePair<bool, string>(false, exception.Message);
			}
		}

		/// <summary>
		/// Downloads the specified source HTTPS URL.
		/// </summary>
		/// <param name="sourceHttpUrl">The source HTTPS URL.</param>
		/// <param name="fullLocalPath">The full local path.</param>
		/// <returns></returns>
		public async Task<bool> Download(string sourceHttpUrl, string fullLocalPath)
		{
			if ( _storage == null ) throw new EndOfStreamException("is null " + nameof(_storage) );

            Uri sourceUri = new Uri(sourceHttpUrl);

            _logger.LogInformation("[Download] HttpClientHelper > " + sourceUri.Host + " ~ " + sourceHttpUrl);

            // allow whitelist and https only
            if ( !AllowedDomains.Contains(sourceUri.Host) ||
                 sourceUri.Scheme != "https" )
            {
	            _logger.LogInformation("[Download] HttpClientHelper > " + "skip: domain not whitelisted " + " ~ " + sourceHttpUrl);
	            return false;
            }
            try
            {
	            using (HttpResponseMessage response = await _httpProvider.GetAsync(sourceHttpUrl))
	            using (Stream streamToReadFrom = await response.Content.ReadAsStreamAsync())
	            {
		            if ( response.StatusCode != HttpStatusCode.OK )
		            {
			            _logger.LogInformation("[Download] HttpClientHelper > " + response.StatusCode + " ~ " + sourceHttpUrl);
			            return false;
		            }

	                await _storage.WriteStreamAsync(streamToReadFrom, fullLocalPath);
	                return true;
	            }
            }
            catch (HttpRequestException exception)
            {
	            _logger.LogError(exception, $"[Download] {exception.Message}");
	            return false;
            }
        }
    }

}
