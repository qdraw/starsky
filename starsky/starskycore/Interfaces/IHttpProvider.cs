using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace starskycore.Interfaces
{
	public interface IHttpProvider
	{
		Task<HttpResponseMessage> GetAsync(string requestUri);
	}
}
