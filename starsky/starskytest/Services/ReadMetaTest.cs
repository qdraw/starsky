using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starsky.foundation.readmeta.Services;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

namespace starskytest.Services
{
	[TestClass]
	public sealed class ReadMetaTest
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
			                            "<photomechanic:ColorClass>8</photomechanic:ColorClass>  " +
			                            "<photomechanic:PMVersion>PM5</photomechanic:PMVersion> " +
			                            " <photomechanic:Prefs>0:8:0:0</photomechanic:Prefs>  " +
			                            "<photomechanic:Tagged>False</photomechanic:Tagged>" +
			                            " </rdf:Description> <rdf:Description rdf:about=\'\' " +
			                            " xmlns:photoshop=\'http://ns.adobe.com/photoshop/1.0/\'> " +
			                            " <photoshop:DateCreated>2019-03-02T11:29:18+01:00</photoshop:DateCreated> " +
			                            "</rdf:Description> <rdf:Description rdf:about=\'\'  xmlns:xmp=\'http://ns.adobe.com/xap/1.0/\'> " +
			                            " <xmp:CreateDate>2019-03-02T11:29:18+01:00</xmp:CreateDate>  <xmp:Rating>0</xmp:Rating> " +
			                            "</rdf:Description></rdf:RDF></x:xmpmeta>";

			byte[] xmpByteArray = Encoding.UTF8.GetBytes(xmpString);

		    
			var fakeIStorage = new FakeIStorage(new List<string> {"/"}, 
				new List<string> {"/test.arw", "/test.xmp"}, new List<byte[]>{CreateAnImage.Bytes.ToArray(),xmpByteArray}  );
		    
			var data = new ReadMeta(fakeIStorage, new AppSettings(),
				null, new FakeIWebLogger()).ReadExifAndXmpFromFile("/test.arw");
		    
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
		
		[TestMethod]
		public void UpdateReadMetaCache_Null()
		{
			var readMeta = new ReadMeta(new FakeIStorage(), new AppSettings(), null, new FakeIWebLogger());
		    
			readMeta.UpdateReadMetaCache(new List<FileIndexItem>
			{
				new FileIndexItem("/test.jpg")
				{
					Tags = "t2"
				}
			});
		    
			Assert.AreEqual(string.Empty,readMeta.ReadExifAndXmpFromFile("/test.jpg").Tags);
		}
		
		[TestMethod]
		public void UpdateReadMetaCache_AppSettingsDisabled()
		{
			var provider = new ServiceCollection()
				.AddMemoryCache()
				.BuildServiceProvider();
			var memoryCache = provider.GetService<IMemoryCache>();
			var readMeta = new ReadMeta(new FakeIStorage(), new AppSettings
			{
				AddMemoryCache = false
			}, memoryCache, new FakeIWebLogger());
		    
			readMeta.UpdateReadMetaCache(new List<FileIndexItem>
			{
				new FileIndexItem("/test.jpg")
				{
					Tags = "t2"
				}
			});
		    
			Assert.AreEqual(string.Empty,readMeta.ReadExifAndXmpFromFile("/test.jpg").Tags);
		}
		
				
		[TestMethod]
		public void RemoveReadMetaCache_Null()
		{
			var readMeta = new ReadMeta(new FakeIStorage(), new AppSettings(), null, new FakeIWebLogger());
		    
			var result = readMeta.RemoveReadMetaCache("/test.jpg");
		    
			Assert.IsNull(result);
		}
		
		[TestMethod]
		public void RemoveReadMetaCache_AppSettingsDisabled()
		{
			var provider = new ServiceCollection()
				.AddMemoryCache()
				.BuildServiceProvider();
			var memoryCache = provider.GetService<IMemoryCache>();
			var readMeta = new ReadMeta(new FakeIStorage(), new AppSettings
			{
				AddMemoryCache = false
			}, memoryCache, new FakeIWebLogger());
		    
			var result = readMeta.RemoveReadMetaCache("/test.jpg");
		    
			Assert.IsNull(result);
		}
		
		[TestMethod]
		public void RemoveReadMetaCache_NotFound()
		{
			var provider = new ServiceCollection()
				.AddMemoryCache()
				.BuildServiceProvider();
			var memoryCache = provider.GetService<IMemoryCache>();
			var readMeta = new ReadMeta(new FakeIStorage(), new AppSettings
			{
				AddMemoryCache = true
			}, memoryCache, new FakeIWebLogger());
		    
			var result = readMeta.RemoveReadMetaCache("/test.jpg");
		    
			Assert.IsFalse(result);
		}
		
		[TestMethod]
		public void RemoveReadMetaCache_Exists()
		{
			var provider = new ServiceCollection()
				.AddMemoryCache()
				.BuildServiceProvider();
			var memoryCache = provider.GetService<IMemoryCache>();
			var readMeta = new ReadMeta(new FakeIStorage(), new AppSettings
			{
				AddMemoryCache = true
			}, memoryCache, new FakeIWebLogger());
			
			// set cache
			readMeta.ReadExifAndXmpFromFile("/test.jpg");

			var result = readMeta.RemoveReadMetaCache("/test.jpg");
		    
			Assert.IsTrue(result);
		}

		[TestMethod]
		public void ReadMetaTest_CheckIfCacheListIsUpdated()
		{
			var provider = new ServiceCollection()
				.AddMemoryCache()
				.BuildServiceProvider();
			var memoryCache = provider.GetService<IMemoryCache>();
			var readMeta = new ReadMeta(new FakeIStorage(), new AppSettings(), memoryCache, new FakeIWebLogger());
		    
			readMeta.UpdateReadMetaCache(new List<FileIndexItem>
			{
				new FileIndexItem("/test.jpg")
				{
					Tags = "t2"
				}
			});
		    
			Assert.AreEqual("t2",readMeta.ReadExifAndXmpFromFile("/test.jpg").Tags);
		}
	    
		[TestMethod]
		public void ReadMetaTest_CheckIfCacheIsUpdated_SingleItem()
		{
			var provider = new ServiceCollection()
				.AddMemoryCache()
				.BuildServiceProvider();
			var memoryCache = provider.GetService<IMemoryCache>();
			var readMeta = new ReadMeta(new FakeIStorage(), new AppSettings(), memoryCache, new FakeIWebLogger());
		    
			readMeta.UpdateReadMetaCache("/test.jpg",
				new FileIndexItem("/test.jpg")
				{
					Tags = "t2"
				}
			);
		    
			Assert.AreEqual("t2",readMeta.ReadExifAndXmpFromFile("/test.jpg").Tags);
		}
		
		[TestMethod]
		public void CorruptXmpFile_SoIgnore()
		{
			var storage = new FakeIStorage(new List<string> { "/" },
				new List<string> { "/test.dng", "/test.xmp" }, new List<byte[]>
				{
					CreateAnImage.Bytes.ToArray(),
					Array.Empty<byte>()
				});
			var readMeta = new ReadMeta(storage, new AppSettings(), null!, new FakeIWebLogger());
	    
			Assert.AreEqual("test, sion",readMeta.ReadExifAndXmpFromFile("/test.dng").Tags);
		}
		
		[TestMethod]
		public void ShouldPickXmpFile()
		{
			var storage = new FakeIStorage(new List<string> { "/" },
				new List<string> { "/test.dng", "/test.xmp" }, new List<byte[]>
				{
					CreateAnImage.Bytes.ToArray(),
					CreateAnXmp.Bytes.ToArray()
				});
			var readMeta = new ReadMeta(storage, new AppSettings(), null!, new FakeIWebLogger());
	    
			Assert.AreEqual(string.Empty,readMeta.ReadExifAndXmpFromFile("/test.dng").Tags);
		}
	}
}
