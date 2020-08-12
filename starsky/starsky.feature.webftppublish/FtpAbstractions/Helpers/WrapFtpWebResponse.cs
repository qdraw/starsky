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

		protected virtual void Dispose(bool disposing)
		{
			if ( !disposing ) return;
			if ( _response == null ) return;
			((IDisposable)_response).Dispose();
			_response = null;
		}

		public Stream GetResponseStream()
		{
			return _response.GetResponseStream();
		}
	}
}
