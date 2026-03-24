using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail;

namespace starskytest.starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail;

[TestClass]
public class LightweightContainerPreviewExtractor_TryResolveAndValidateOffsetTests
{
    private const int MinJpegSize = 4096;

    [TestMethod]
    public void TryResolveAndValidateOffset_ReturnsFalse_When_CandidateOffsetZero()
    {
        using var ms = new MemoryStream(new byte[1000]);
        var ok = LightweightContainerPreviewExtractor.TryResolveAndValidateOffset(ms, 0, 0u, MinJpegSize, out var resolved);
        Assert.IsFalse(ok);
        Assert.AreEqual(0u, resolved);
    }

    [TestMethod]
    public void TryResolveAndValidateOffset_ReturnsFalse_When_CandidateLengthTooSmall()
    {
        using var ms = new MemoryStream(new byte[2000]);
        var ok = LightweightContainerPreviewExtractor.TryResolveAndValidateOffset(ms, 0, 100u, MinJpegSize - 1, out var resolved);
        Assert.IsFalse(ok);
        Assert.AreEqual(0u, resolved);
    }

    [TestMethod]
    public void TryResolveAndValidateOffset_ReturnsTrue_When_CandidateOffsetValid()
    {
        var candidateOffset = 100u;
        var candidateLength = (uint)MinJpegSize;
        // Create a buffer large enough and place JPEG marker at candidateOffset
        var buf = new byte[candidateOffset + candidateLength];
        buf[candidateOffset + 0] = 0xFF;
        buf[candidateOffset + 1] = 0xD8;
        buf[candidateOffset + 2] = 0xFF;
        using var ms = new MemoryStream(buf);

        var ok = LightweightContainerPreviewExtractor.TryResolveAndValidateOffset(ms, 0, candidateOffset, candidateLength, out var resolved);
        Assert.IsTrue(ok);
        Assert.AreEqual(candidateOffset, resolved);
    }

    [TestMethod]
    public void TryResolveAndValidateOffset_ReturnsTrue_When_RelativeOffsetValid()
    {
        const int tiffBase = 1000;
        const uint candidateOffset = 200u; // candidate offset alone will not have marker
        const uint candidateLength = MinJpegSize;
        const uint relativeOffset = tiffBase + (int)candidateOffset;

        // Create buffer with marker only at relativeOffset
        var buf = new byte[relativeOffset + candidateLength];
        buf[relativeOffset + 0] = 0xFF;
        buf[relativeOffset + 1] = 0xD8;
        buf[relativeOffset + 2] = 0xFF;
        using var ms = new MemoryStream(buf);

        var ok = LightweightContainerPreviewExtractor.TryResolveAndValidateOffset(ms, tiffBase, candidateOffset, candidateLength, out var resolved);
        Assert.IsTrue(ok);
        Assert.AreEqual(relativeOffset, resolved);
    }

    [TestMethod]
    public void TryResolveAndValidateOffset_ReturnsFalse_When_BothOffsetsInvalid()
    {
        var tiffBase = 500;
        var candidateOffset = 300u;
        var candidateLength = (uint)MinJpegSize;
        // Create buffer that is too small so offset+length > length
        var buf = new byte[400];
        using var ms = new MemoryStream(buf);

        var ok = LightweightContainerPreviewExtractor.TryResolveAndValidateOffset(ms, tiffBase, candidateOffset, candidateLength, out var resolved);
        Assert.IsFalse(ok);
        Assert.AreEqual(0u, resolved);
    }
}

