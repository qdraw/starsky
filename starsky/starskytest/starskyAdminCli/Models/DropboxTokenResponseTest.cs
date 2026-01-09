using Microsoft.VisualStudio.TestTools.UnitTesting;
using starskyAdminCli.Models;

namespace starskytest.starskyAdminCli.Models;

[TestClass]
public class DropboxTokenResponseTest
{
	[TestMethod]
	public void DropboxTokenResponseTestMatch()
	{
		var token = new DropboxTokenResponse
		{
			AccessToken = "123", ExpiresIn = 123, RefreshToken = "456", TokenType = "789"
		};
		Assert.AreEqual("123", token.AccessToken);
		Assert.AreEqual(123, token.ExpiresIn);
		Assert.AreEqual("456", token.RefreshToken);
		Assert.AreEqual("789", token.TokenType);
	}
}
