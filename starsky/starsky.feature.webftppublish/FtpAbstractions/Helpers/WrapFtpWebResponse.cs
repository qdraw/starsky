using System;
using System.IO;
using System.Net;
using starsky.feature.webftppublish.FtpAbstractions.Interfaces;

namespace starsky.feature.webftppublish.FtpAbstractions.Helpers
{
	public class WrapFtpWebResponse : IFtpWebResponse
	{
		private WebResponse _response;

		public WrapFtpWebResponse(FtpWebResponse response)
		{
			_response = response;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (_response != null)
				{
					((IDisposable)_response).Dispose();
					_response = null;
				}
			}
		}

		public Stream GetResponseStream()
		{
			return _response.GetResponseStream();
		}
	}
}
