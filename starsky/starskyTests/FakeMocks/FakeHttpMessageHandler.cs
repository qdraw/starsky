using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace starskytests.FakeMocks
{
	public class FakeHttpMessageHandler : HttpMessageHandler
	{
		public virtual HttpResponseMessage Send(HttpRequestMessage request)
		{
			return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("Your message here") };
			// Configure this method however you wish for your testing needs.
		}

		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			return Task.FromResult(Send(request));
		}
	}
}
