using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.thumbnailgeneration.Models;

namespace starskytest.starsky.foundation.thumbnailgeneration.Models;

[TestClass]
public class GenerationResultModelTest
{
	[TestMethod]
	public void GenerationResultModel1()
	{
		var model = new GenerationResultModel { SizeInPixels = 300 };

		Assert.AreEqual(300, model.SizeInPixels);
	}
}
