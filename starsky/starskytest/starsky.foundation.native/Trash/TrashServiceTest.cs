using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.native.Helpers;
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
	public void TrashService_Trash()
	{
		var result = new TrashService().Trash("test");
		
		// This feature is not working on Linux and FreeBSD
		if ( OperatingSystemHelper.GetPlatform() == OSPlatform.Linux || 
		     OperatingSystemHelper.GetPlatform() == OSPlatform.FreeBSD )
		{
			Assert.IsNull(result);
			return;
		}
		
		Assert.IsTrue(result);
	}
}
