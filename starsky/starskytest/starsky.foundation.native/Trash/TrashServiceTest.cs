using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.native.Trash;

namespace starskytest.starsky.foundation.native.Trash;

[TestClass]
public class TrashServiceTest
{
	[TestMethod]
	public void TrashService_CanUseSystemTrash1()
	{
		var result = new TrashService().DetectToUseSystemTrash();
		Assert.IsNotNull(result);
	}
	
	[TestMethod]
	public void TrashService_Trash1()
	{
		var result = new TrashService().Trash("test");
		Assert.IsNotNull(result);
	}
}
