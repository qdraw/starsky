using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Controllers;
using starsky.foundation.database.Models.Account;
using starskytest.FakeMocks;

namespace starskytest.Controllers;

[TestClass]
public sealed class MemoryCacheDebugControllerCredentialTest
{
	[TestMethod]
	public void MemoryCacheDebug_CachedCredential_DoesNotExposeSecretOrSalt()
	{
		var memoryCache = new ServiceCollection().AddMemoryCache().BuildServiceProvider()
			.GetRequiredService<IMemoryCache>();

		// UserManager.CachedCredential stores the full Credential (hash + salt) in this cache
		memoryCache.Set("any-key", new Credential
		{
			Identifier = "user@example.test", Secret = "PBKDF2-HASH-VALUE", Extra = "SALT-VALUE"
		});
		memoryCache.Set("other-debug-key", "visible debug value");

		var controller = new MemoryCacheDebugController(memoryCache, new FakeIWebLogger());
		var result = ( controller.MemoryCacheDebug() as JsonResult )?.Value as
			Dictionary<string, object>;

		Assert.IsNotNull(result);
		var all = string.Join("|", result.Values.Select(p => p.ToString()));
		Assert.DoesNotContain("PBKDF2-HASH-VALUE", all);
		Assert.DoesNotContain("SALT-VALUE", all);
		// other cache entries stay useful for debugging
		Assert.Contains("visible debug value", all);
	}
}
