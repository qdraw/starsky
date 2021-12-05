using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace starskytest.FakeMocks
{
	public class FakeHttpMessageHandler : HttpMessageHandler
	{
		private readonly Exception _exception;

		public FakeHttpMessageHandler(Exception exception = null)
		{
			_exception = exception;
		}
		
		public virtual HttpResponseMessage Send(HttpRequestMessage request)
		{
			if ( _exception != null ) throw _exception;
			if ( request.RequestUri.Host == "download.geonames.org" )
			{
				return new HttpResponseMessage(HttpStatusCode.NotFound) { Content = new StringContent("404") };

			}
			return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("Your message here") };
			// Configure this method however you wish for your testing needs.
		}

		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			return Task.FromResult(Send(request));
		}
	}
}
