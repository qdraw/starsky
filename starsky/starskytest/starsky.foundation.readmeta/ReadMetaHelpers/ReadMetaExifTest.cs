using System.Collections.Generic;
using MetadataExtractor;
using MetadataExtractor.Formats.Xmp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.readmeta.ReadMetaHelpers;
using XmpCore.Impl;

namespace starskytest.starsky.foundation.readmeta.ReadMetaHelpers;

[TestClass]
public class ReadMetaExifTest
{
    [TestMethod]
    public void GetXmpGeoAlt_PositiveAltitude_ReturnsPositiveAltitude()
    {
        // Arrange
        var dir2 = new XmpDirectory();
        dir2.SetXmpMeta(new XmpMeta());
        dir2.XmpMeta?.SetProperty("http://ns.adobe.com/exif/1.0/", "GPSAltitude","1/1" );
        dir2.XmpMeta?.SetProperty("http://ns.adobe.com/exif/1.0/", "GPSAltitudeRef","0" );

        var allExifItems = new List<Directory>
        {
	        dir2
        };

        // Act
        var result = ReadMetaExif.GetXmpGeoAlt(allExifItems);

        // Assert
        Assert.AreEqual(1, result, 0.001); // Use an appropriate tolerance
    }

    [TestMethod]
    public void GetXmpGeoAlt_NegativeAltitude_ReturnsNegativeAltitude()
    {
        // Arrange
        var dir2 = new XmpDirectory();
        dir2.SetXmpMeta(new XmpMeta());
        dir2.XmpMeta?.SetProperty("http://ns.adobe.com/exif/1.0/", "GPSAltitude","10/1" );
        dir2.XmpMeta?.SetProperty("http://ns.adobe.com/exif/1.0/", "GPSAltitudeRef","1" );

        var allExifItems = new List<Directory>
        {
	        dir2
        };

        // Act
        var result = ReadMetaExif.GetXmpGeoAlt(allExifItems);

        // Assert
        Assert.AreEqual(-10, result, 0.001); // Use an appropriate tolerance
    }

    [TestMethod]
    public void GetXmpGeoAlt_ZeroAltitude_ReturnsZeroAltitude()
    {
        // Arrange
        var dir2 = new XmpDirectory();
        dir2.SetXmpMeta(new XmpMeta());
        dir2.XmpMeta?.SetProperty("http://ns.adobe.com/exif/1.0/", "GPSAltitude","0/1" );
        dir2.XmpMeta?.SetProperty("http://ns.adobe.com/exif/1.0/", "GPSAltitudeRef","1" );
        var allExifItems = new List<Directory>
        {
	        dir2
        };
        
        // Act
        var result = ReadMetaExif.GetXmpGeoAlt(allExifItems);

        // Assert
        Assert.AreEqual(0, result, 0.001); // Use an appropriate tolerance
    }

}
