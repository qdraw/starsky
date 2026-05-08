using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.thumbnailgeneration.GenerationFactory.RawDng;

namespace starskytest.starsky.foundation.thumbnailgeneration.GenerationFactory.RawDng;

[TestClass]
public class PipelineTests
{
	[TestMethod]
	public void Run_ExecutesStepsInOrder()
	{
		var pipeline = new Pipeline<int>()
			.Add(x => x + 1)
			.Add(x => x * 2)
			.Add(x => x - 3);

		var result = pipeline.Run(4);

		Assert.AreEqual(7, result); // ((4 + 1) * 2) - 3
	}

	[TestMethod]
	public void Run_WithNoSteps_ReturnsInput()
	{
		var pipeline = new Pipeline<string>();

		var result = pipeline.Run("raw");

		Assert.AreEqual("raw", result);
	}
}

