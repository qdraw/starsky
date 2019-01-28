using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using starskycore.Interfaces;

namespace starskycore.Helpers
{
    public class HttpClientHelper
    {
	    public HttpClientHelper(HttpProvider httpProvider)
	    {
		    _httpProvider = httpProvider;
	    }

	    private readonly HttpProvider _httpProvider;


		/// <summary>
		/// This domains are only allowed domains to download from (and https only)
		/// </summary>
		private readonly List<string> AllowedDomains = new List<string>
        {
            "dl.dropboxusercontent.com", 
            "qdraw.nl", 
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
                
                using (Stream streamToWriteTo = File.Open(fullLocalPath, FileMode.Create))
                {
                    await streamToReadFrom.CopyToAsync(streamToWriteTo);
                    return true;
                }
            }
        }
    }
}
