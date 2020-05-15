using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using starsky.foundation.http.Interfaces;

namespace starsky.foundation.writemeta.Helpers
{
	public class ExifToolDownload
	{
		private readonly IHttpClientHelper _httpClientHelper;

		public ExifToolDownload(IHttpClientHelper httpClientHelper )
		{
			_httpClientHelper = httpClientHelper;
		}

		public async Task DownloadExifToolForWindows()
		{
			var checksums = await _httpClientHelper.ReadString("https://exiftool.org/checksums.txt");
			if ( !checksums.Key )
			{
				return;
			}
			// (?<=SHA1\()exiftool-[\d\.]+\.zip
			var regexExifToolForWindowsName = new Regex(@"(?<=SHA1\()exiftool-[0-9\.]+\.zip");

			var match = regexExifToolForWindowsName.Match(checksums.Value);
			Console.WriteLine(match);
		}
	}
}
