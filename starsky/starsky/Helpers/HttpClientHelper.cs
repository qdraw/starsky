using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace starsky.Helpers
{
    public static class HttpClientHelper
    {
        static readonly List<string> AllowedDomains = new List<string> {"dl.dropboxusercontent.com", "qdraw.nl"};
        
        private static readonly HttpClient Client = new HttpClient();
        public static async Task<bool> Download(string sourceHttpUrl, string fullLocalPath) 
        {
            Uri sourceUri = new Uri(sourceHttpUrl);

            // allow whitelist and https only
            if (!AllowedDomains.Contains(sourceUri.Host) || sourceUri.Scheme != "https") return false;
            
            using (HttpResponseMessage response = await Client.GetAsync(sourceHttpUrl, HttpCompletionOption.ResponseHeadersRead))
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