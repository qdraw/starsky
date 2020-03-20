using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;
using starsky.foundation.readmeta.Services;
using starskycore.Models;
using starskycore.Services;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

namespace starskytest.Services
{
	[TestClass]
	public class ReadMetaTest
	{
		[TestMethod]
	    public void ReadMetaTest_HalfCompleteFile()
	    {
		    const string xmpString = 	"<x:xmpmeta xmlns:x=\'adobe:ns:meta/\' x:xmptk=\'Image::ExifTool 11.11\'>" +
										"<rdf:RDF xmlns:rdf=\'http://www.w3.org/1999/02/22-rdf-syntax-ns#\'>" +
										" <rdf:Description rdf:about=\'\'  xmlns:dc=\'http://purl.org/dc/elements/1.1/\'> " +
										" <dc:subject>   <rdf:Bag>    <rdf:li>example</rdf:li>    <rdf:li>keyword</rdf:li>  " +
										"  <rdf:li>test</rdf:li>   </rdf:Bag>  </dc:subject> </rdf:Description> " +
										"<rdf:Description rdf:about=\'\'  xmlns:pdf=\'http://ns.adobe.com/pdf/1.3/\'>  " +
										"<pdf:Keywords>example, test</pdf:Keywords> </rdf:Description> <rdf:Description rdf:about=\'\' " +
										" xmlns:photomechanic=\'http://ns.camerabits.com/photomechanic/1.0/\'>  " +
										"<photomechanic:ColorClass>8</photomechanic:ColorClass>  <photomechanic:PMVersion>PM5</photomechanic:PMVersion> " +
										" <photomechanic:Prefs>0:8:0:0</photomechanic:Prefs>  <photomechanic:Tagged>False</photomechanic:Tagged>" +
										" </rdf:Description> <rdf:Description rdf:about=\'\'  xmlns:photoshop=\'http://ns.adobe.com/photoshop/1.0/\'> " +
										" <photoshop:DateCreated>2019-03-02T11:29:18+01:00</photoshop:DateCreated> " +
										"</rdf:Description> <rdf:Description rdf:about=\'\'  xmlns:xmp=\'http://ns.adobe.com/xap/1.0/\'> " +
										" <xmp:CreateDate>2019-03-02T11:29:18+01:00</xmp:CreateDate>  <xmp:Rating>0</xmp:Rating> " +
										"</rdf:Description></rdf:RDF></x:xmpmeta>";

		    byte[] xmpByteArray = Encoding.UTF8.GetBytes(xmpString);

		    
		    var fakeIStorage = new FakeIStorage(new List<string> {"/"}, new List<string> {"/test.arw", "/test.xmp"}, new List<byte[]>{CreateAnImage.Bytes,xmpByteArray}  );
		    
		    var data = new ReadMeta(fakeIStorage).ReadExifAndXmpFromFile("/test.arw");
		    
		    // Is in source file
		    Assert.AreEqual(200,data.IsoSpeed);
		    Assert.AreEqual("Diepenveen",data.LocationCity);
		    Assert.AreEqual("caption",data.Description);

		    // Words overwritten in xmp file
		    Assert.AreEqual("example, keyword, test",data.Tags);
		    
		    DateTime.TryParseExact("2019-03-02T11:29:18+01:00",
			    "yyyy-MM-dd\\THH:mm:sszzz",
			    CultureInfo.InvariantCulture,
			    DateTimeStyles.None,
			    out var expectDateTime);
		    
		    Assert.AreEqual(expectDateTime,data.DateTime);
		    Assert.AreEqual(ColorClassParser.Color.Trash,data.ColorClass);

	    }
	}
}
