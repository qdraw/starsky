using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.native.Trash;

namespace starskytest.starsky.foundation.native.Trash;

[TestClass]
public class CanUseSystemTrashTest
{
	[TestMethod]
	public void CanUseSystemTrash1()
	{
		var result = CanUseSystemTrash.UseTrash();
		Assert.IsNotNull(result);
	}
}
