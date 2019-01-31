using System;
using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starskycore.Helpers;
using starskycore.Models;
using starskycore.Services;
using starskytests.FakeCreateAn;

namespace starskytests.Services
{
    [TestClass]
    public class XmpReadHelperTest
    {
        private const string Input = "<x:xmpmeta xmlns:x=\"adobe:ns:meta/\" x:xmptk=\"XMP Core 5.1.2\"> <rdf:RDF xmlns:rdf=\""+
                        "http://www.w3.org/1999/02/22-rdf-syntax-ns#\"> <rdf:Description rdf:about=\"\" "+
                        "xmlns:aux=\"http://ns.adobe.com/exif/1.0/aux/\" xmlns:crs=\"http://ns.adobe.com/camera-raw-settings/1.0/\" "+
                        "xmlns:exif=\"http://ns.adobe.com/exif/1.0/\" xmlns:exifEX=\"http://cipa.jp/exif/1.0/\" xmlns:pdf=\"http://ns.adobe.com/pdf/1.3/\" "+
                        "xmlns:photoshop=\"http://ns.adobe.com/photoshop/1.0/\" xmlns:pmi=\"http://prismstandard.org/namespaces/pmi/2.2/\" xmlns:tiff=\"http://ns.adobe.com/tiff/1.0/\" "+
                        "xmlns:xmp=\"http://ns.adobe.com/xap/1.0/\" xmlns:dc=\"http://purl.org/dc/elements/1.1/\" xmlns:Iptc4xmpExt=\"http://iptc.org/std/Iptc4xmpExt/2008-02-29/\" "+
                        "xmlns:photomechanic=\"http://ns.camerabits.com/photomechanic/1.0/\" aux:LensID=\"24\" crs:Brightness=\"0\" crs:Temperature=\"0\" exif:BrightnessValue=\"61/40\" "+
                        "exif:ColorSpace=\"1\" exif:CompressedBitsPerPixel=\"8/1\" exif:Contrast=\"0\" exif:CustomRendered=\"0\" exif:DateTimeOriginal=\"2018-07-18T19:44:27\" "+
                        "exif:DigitalZoomRatio=\"1/1\" exif:ExifVersion=\"0230\" exif:ExposureBiasValue=\"-3/10\" exif:ExposureMode=\"0\" exif:ExposureProgram=\"3\" "+
                        "exif:ExposureTime=\"1/13\" exif:FNumber=\"9/1\" exif:FileSource=\"3\" exif:FlashpixVersion=\"0100\" exif:FocalLength=\"28/1\" exif:FocalLengthIn35mmFilm=\"42\" exif:GPSAltitude=\"190/10\" "+
                        "exif:GPSAltitudeRef=\"0\" exif:GPSLatitude=\"52,20.708N\" exif:GPSLongitude=\"5,55.840E\" exif:GPSMapDatum=\"WGS-84\" exif:GPSTimeStamp=\"2018-07-18T17:44:20Z\" exif:GPSVersionID=\"2.2.0.0\" "+
                        "exif:LightSource=\"0\" exif:MaxApertureValue=\"16205/3734\" exif:MeteringMode=\"5\" exif:PixelXDimension=\"5456\" exif:PixelYDimension=\"3632\" exif:Saturation=\"0\" exif:SceneCaptureType=\"0\" "+
                        "exif:SceneType=\"1\" exif:Sharpness=\"0\" exif:WhiteBalance=\"0\" exif:GPSDateTime=\"2018-07-18T17:44:20Z\" exifEX:InteroperabilityIndex=\"R98\" exifEX:LensModel=\"24-105mm F3.5-4.5\" "+
                        "exifEX:RecommendedExposureIndex=\"800\" exifEX:SensitivityType=\"2\" pdf:Keywords=\"zwavelzwam, update1235\" photoshop:ColorMode=\"0\" photoshop:City=\"Epe\" photoshop:State=\"Gelderland\" "+
                        "photoshop:Country=\"Nederland\" photoshop:DateCreated=\"2018-07-18T19:44:27+01:00\" pmi:sequenceNumber=\"1\" tiff:Compression=\"32767\" tiff:ImageLength=\"3656\" tiff:ImageWidth=\"5504\" "+
                        "tiff:Make=\"SONY\" tiff:Model=\"SLT-A58\" tiff:Orientation=\"1\" tiff:PhotometricInterpretation=\"32803\" tiff:PlanarConfiguration=\"1\" tiff:ResolutionUnit=\"2\" tiff:SamplesPerPixel=\"1\" "+
                        "tiff:XResolution=\"350/1\" tiff:YCbCrPositioning=\"2\" tiff:YResolution=\"350/1\" xmp:CreateDate=\"2018-07-18T19:44:27\" xmp:CreatorTool=\"SLT-A58 v1.00\" xmp:MetadataDate=\"2018-07-23T10:41:22+02:00\" "+
                        "xmp:ModifyDate=\"2018-07-18T19:44:27\" xmp:Label=\"\" xmp:Rating=\"0\" photomechanic:ColorClass=\"1\" photomechanic:Tagged=\"False\" photomechanic:Prefs=\"0:1:0:-00001\" photomechanic:PMVersion=\"PM5\"> "+
                        "<exif:ComponentsConfiguration> <rdf:Seq> <rdf:li>1</rdf:li> <rdf:li>2</rdf:li> <rdf:li>3</rdf:li> <rdf:li>0</rdf:li> </rdf:Seq> </exif:ComponentsConfiguration> <exif:Flash exif:Fired=\"False\" "+
                        "exif:Function=\"False\" exif:Mode=\"2\" exif:RedEyeMode=\"False\" exif:Return=\"0\"/> <exif:ISOSpeedRatings> <rdf:Seq> <rdf:li>800</rdf:li> </rdf:Seq> </exif:ISOSpeedRatings> <exif:UserComment> <rdf:Alt>"+
                        " <rdf:li xml:lang=\"x-default\"/> </rdf:Alt> </exif:UserComment> <exifEX:LensSpecification> <rdf:Seq> <rdf:li>24/1</rdf:li> <rdf:li>105/1</rdf:li> "+
                        "<rdf:li>7/2</rdf:li> <rdf:li>9/2</rdf:li> </rdf:Seq> </exifEX:LensSpecification>"+ 
                        " <tiff:BitsPerSample> <rdf:Seq> <rdf:li>12</rdf:li> </rdf:Seq> "+
                        "</tiff:BitsPerSample> <dc:subject> <rdf:Bag> <rdf:li>keyword</rdf:li> <rdf:li>keyword2</rdf:li> </rdf:Bag> </dc:subject> <dc:description> <rdf:Alt> "+
                        "<rdf:li xml:lang=\"x-default\">caption</rdf:li> </rdf:Alt> </dc:description> <dc:title> <rdf:Alt> <rdf:li xml:lang=\"x-default\">The object name</rdf:li>"+
                        " </rdf:Alt> </dc:title> <Iptc4xmpExt:LocationCreated> <rdf:Bag>"+ " <rdf:li Iptc4xmpExt:Sublocation=\"\" Iptc4xmpExt:City=\"Epe\" "+
                        "Iptc4xmpExt:ProvinceState=\"Gelderland\" Iptc4xmpExt:CountryName=\"Nederland\" Iptc4xmpExt:CountryCode=\"\" Iptc4xmpExt:WorldRegion=\"\"/> </rdf:Bag> "+
                        "</Iptc4xmpExt:LocationCreated> <Iptc4xmpExt:LocationShown> <rdf:Bag> <rdf:li Iptc4xmpExt:Sublocation=\"\" Iptc4xmpExt:City=\"Epe\" "+
                        "Iptc4xmpExt:ProvinceState=\"Gelderland\" Iptc4xmpExt:CountryName=\"Nederland\" "+
                        "Iptc4xmpExt:CountryCode=\"\" Iptc4xmpExt:WorldRegion=\"\"/> </rdf:Bag> </Iptc4xmpExt:LocationShown> </rdf:Description> </rdf:RDF> </x:xmpmeta>";


        [TestMethod]
        public void XmpReadHelperTest_GetData_usingStringExample()
        {
            var data = new ReadMeta(new AppSettings()).GetDataFromString(Input);
            
            Assert.AreEqual(52.3451333333,data.Latitude,0.001);
            Assert.AreEqual(5.930,data.Longitude,0.001);
            Assert.AreEqual(19,data.LocationAltitude,0.001);

            Assert.AreEqual("caption",data.Description);
            Assert.AreEqual("keyword, keyword2",data.Tags);
            Assert.AreEqual("The object name",data.Title);
            
            Assert.AreEqual("Epe",data.LocationCity);
            Assert.AreEqual("Gelderland",data.LocationState);
            Assert.AreEqual("Nederland",data.LocationCountry);

            Assert.AreEqual(FileIndexItem.Color.Winner,data.ColorClass);
            
            DateTime.TryParseExact("2018-07-18 19:44:27", 
                "yyyy-MM-dd HH:mm:ss",
                CultureInfo.InvariantCulture, 
                DateTimeStyles.None, 
                out var dateTime);
            Assert.AreEqual(dateTime, data.DateTime);
            
        }

        [TestMethod]
        public void XmpReadHelperTest_XmpGetSidecarFile()
        {
            var createAnImage = new CreateAnImage();
            var xmpPath = createAnImage.FullFilePath.Replace("jpg", "xmp");
	        Files.DeleteFile(xmpPath);

            var fakeRawPath = createAnImage.FullFilePath.Replace("jpg", "dng");
            new PlainTextFileHelper().WriteFile(xmpPath,Input);
            
            var appsettings = new AppSettings();

            var databaseItem = new FileIndexItem();
            var readXmp = new ReadMeta(appsettings).XmpGetSidecarFile(databaseItem, fakeRawPath);
            Assert.AreEqual("The object name",readXmp.Title);
            // clean afterwards
            Files.DeleteFile(xmpPath);
        }

    }
}
