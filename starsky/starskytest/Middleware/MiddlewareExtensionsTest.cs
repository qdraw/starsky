using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.accountmanagement.Middleware;

namespace starskytest.Middleware
{

	[TestClass]
	public sealed class MiddlewareExtensionsTest
	{
        
		[TestMethod]
		public async Task MiddlewareExtensionsBasicAuthenticationMiddlewareNotSignedIn()
		{
			// Arrange
			var httpContext = new DefaultHttpContext();
			var authMiddleware = new BasicAuthenticationMiddleware(next: (_) => Task.CompletedTask);

			// Act
			await authMiddleware.Invoke(httpContext);
			
			Assert.IsNotNull(httpContext);
		}
	}
}
