using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail;

namespace starskytest.starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail;

[TestClass]
public class LightweightContainerPreviewExtractor_HandleIfdEntryTests
{
    [TestMethod]
    public void HandleIfdEntry_Compression_Type3_LittleEndian_UsesLowWord()
    {
        ushort compression = 0;
        uint jpegOffset = 0;
        uint jpegLength = 0;

        // value = 0xAABBCCDD -> low word 0xCCDD
        const uint value = 0xAABBCCDDu;
        LightweightContainerPreviewExtractor.HandleIfdEntry(0x0103, 3, value, true, ref compression, ref jpegOffset, ref jpegLength);

        Assert.AreEqual((ushort)0xCCDD, compression);
        Assert.AreEqual(0u, jpegOffset);
        Assert.AreEqual(0u, jpegLength);
    }

    [TestMethod]
    public void HandleIfdEntry_Compression_Type3_BigEndian_UsesHighWord()
    {
        ushort compression = 0;
        uint jpegOffset = 0;
        uint jpegLength = 0;

        // value = 0xAABBCCDD -> high word 0xAABB
        var value = 0xAABBCCDDu;
        LightweightContainerPreviewExtractor.HandleIfdEntry(0x0103, 3, value, false, ref compression, ref jpegOffset, ref jpegLength);

        Assert.AreEqual((ushort)0xAABB, compression);
        Assert.AreEqual(0u, jpegOffset);
        Assert.AreEqual(0u, jpegLength);
    }

    [TestMethod]
    public void HandleIfdEntry_Compression_TypeNot3_UsesValueAsUShort()
    {
        ushort compression = 0xFFFF;
        uint jpegOffset = 0;
        uint jpegLength = 0;

        var value = 7u;
        LightweightContainerPreviewExtractor.HandleIfdEntry(0x0103, 4, value, true, ref compression, ref jpegOffset, ref jpegLength);

        Assert.AreEqual((ushort)7, compression);
    }

    [TestMethod]
    public void HandleIfdEntry_Sets_JpegOffset()
    {
        ushort compression = 0;
        uint jpegOffset = 0;
        uint jpegLength = 0;

        const uint value = 0x12345678u;
        LightweightContainerPreviewExtractor.HandleIfdEntry(0x0201, 4, value, true, ref compression, ref jpegOffset, ref jpegLength);

        Assert.AreEqual(0x12345678u, jpegOffset);
        Assert.AreEqual(0u, jpegLength);
    }

    [TestMethod]
    public void HandleIfdEntry_UnknownTag_DoesNothing()
    {
        ushort compression = 1;
        uint jpegOffset = 2;
        uint jpegLength = 3;

        LightweightContainerPreviewExtractor.HandleIfdEntry(0xFFFF, 4, 100u, true, ref compression, ref jpegOffset, ref jpegLength);

        Assert.AreEqual((ushort)1, compression);
        Assert.AreEqual(2u, jpegOffset);
        Assert.AreEqual(3u, jpegLength);
    }
}

