using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.http.Interfaces;
using starsky.foundation.injection;
using starsky.foundation.platform.Models;
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
	    /// <param name="serviceScopeFactory">Scope contains a IStorageSelector</param>
	    public HttpClientHelper(IHttpProvider httpProvider, IServiceScopeFactory serviceScopeFactory)
	    {
		    _httpProvider = httpProvider;

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

		/// <summary>
		/// This domains are only allowed domains to download from (and https only)
		/// </summary>
		private readonly List<string> AllowedDomains = new List<string>
        {
            "dl.dropboxusercontent.com", 
            "qdraw.nl", // < used by test
            "locker.ifttt.com",
			"download.geonames.org"
		};

		/// <summary>
		/// Downloads the specified source HTTPS URL.
		/// </summary>
		/// <param name="sourceHttpUrl">The source HTTPS URL.</param>
		/// <param name="fullLocalPath">The full local path.</param>
		/// <returns></returns>
		public async Task<bool> Download(string sourceHttpUrl, string fullLocalPath) 
        {
            Uri sourceUri = new Uri(sourceHttpUrl);

            Console.WriteLine("HttpClientHelper > " + sourceUri.Host + " ~ " + sourceHttpUrl);

            // allow whitelist and https only
            if (!AllowedDomains.Contains(sourceUri.Host) || sourceUri.Scheme != "https") return false;
            
            using (HttpResponseMessage response = await _httpProvider.GetAsync(sourceHttpUrl))
            using (Stream streamToReadFrom = await response.Content.ReadAsStreamAsync())
            {
                if (response.StatusCode != HttpStatusCode.OK) return false;

                await _storage.WriteStreamAsync(streamToReadFrom, fullLocalPath);
                return true;
            }
        }
    }

}
