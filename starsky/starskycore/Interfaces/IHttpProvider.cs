using System.Net.Http;
using System.Threading.Tasks;

namespace starskycore.Interfaces
{
	public interface IHttpProvider
	{
		Task<HttpResponseMessage> GetAsync(string requestUri);
	}
}
