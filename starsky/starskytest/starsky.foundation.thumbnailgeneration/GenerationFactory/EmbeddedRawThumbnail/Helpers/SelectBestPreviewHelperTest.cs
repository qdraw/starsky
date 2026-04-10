using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail.Helpers;
using starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail.Models;

namespace starskytest.starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail.
	Helpers;

[TestClass]
public class SelectBestPreviewHelperTest
{
	[TestMethod]
	public void SelectBestPreview_MixedDimensions_KeepsUnknownWhenUnknownLengthIsAtLeastDouble()
	{
		var unknownDimensions = new PreviewCandidate
		{
			Offset = 100, Length = 10_000, Width = 0, Height = 0
		};

		var knownDimensions = new PreviewCandidate
		{
			Offset = 200, Length = 4_000, Width = 1200, Height = 800
		};

		var best = SelectBestPreviewHelper.SelectBestPreview(
		[
			unknownDimensions,
			knownDimensions
		]);

		Assert.IsNotNull(best);
		Assert.AreEqual(unknownDimensions.Offset, best.Offset,
			"Unknown-dimension candidate should stay selected when it is >= 2x the known candidate length.");
	}

	[TestMethod]
	public void SelectBestPreview_MixedDimensions_ReplacesUnknownWhenUnknownLengthIsNotDouble()
	{
		var unknownDimensions = new PreviewCandidate
		{
			Offset = 300, Length = 7_000, Width = 0, Height = 0
		};

		var knownDimensions = new PreviewCandidate
		{
			Offset = 400, Length = 4_000, Width = 1200, Height = 800
		};

		var best = SelectBestPreviewHelper.SelectBestPreview(
		[
			unknownDimensions,
			knownDimensions
		]);

		Assert.IsNotNull(best);
		Assert.AreEqual(knownDimensions.Offset, best.Offset,
			"Known-dimension candidate should replace unknown-dimension candidate when unknown is < 2x length.");
	}
}
