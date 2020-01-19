using System.Threading.Tasks;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;

namespace starskytest.FakeMocks
{
	public class FakeAntiforgery : IAntiforgery
	{
		public AntiforgeryTokenSet GetAndStoreTokens(HttpContext httpContext)
		{
			var result =  new AntiforgeryTokenSet("requestToken","cookieToken","test","test");
			return result;
		}

		public AntiforgeryTokenSet GetTokens(HttpContext httpContext)
		{
			throw new System.NotImplementedException();
		}

		public Task<bool> IsRequestValidAsync(HttpContext httpContext)
		{
			throw new System.NotImplementedException();
		}

		public void SetCookieTokenAndHeader(HttpContext httpContext)
		{
			throw new System.NotImplementedException();
		}

		public Task ValidateRequestAsync(HttpContext httpContext)
		{
			throw new System.NotImplementedException();
		}
	}
}
