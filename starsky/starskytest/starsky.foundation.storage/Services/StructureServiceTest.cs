using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Services;
using starsky.foundation.storage.Storage;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.storage.Services
{
	[TestClass]
	public class StructureServiceTest
	{
		
		[TestMethod]
		public void ParseFileName_DefaultDate()
		{
			var structure = "/yyyy/MM/yyyy_MM_dd/yyyyMMdd_HHmmss.ext";
			var importItem = new StructureService(new FakeIStorage(), structure);
			var fileName = importItem.ParseFileName(new DateTime(), string.Empty, ExtensionRolesHelper.ImageFormat.jpg );
			Assert.AreEqual("00010101_000000.jpg", fileName);
		}
		
		[TestMethod]
		public void ParseFileName_LotsOfEscapeChars()
		{
			var structure = "/yyyyMMdd_HHmmss_\\\\\\h\\\\\\m.ext";
			var importItem = new StructureService(new FakeIStorage(), structure);
			var fileName = importItem.ParseFileName(new DateTime(), string.Empty, ExtensionRolesHelper.ImageFormat.jpg );
			Assert.AreEqual("00010101_000000_hm.jpg", fileName);
		}
		
		[TestMethod]
		public void ParseFileName_FileNameWithAppendix()
		{
			var structure = "/yyyy/MM/yyyy_MM_dd/yyyyMMdd_HHmmss_\\d.ext";
			var importItem = new StructureService(new FakeIStorage(), structure);
			var fileName = importItem.ParseFileName(new DateTime(), string.Empty, ExtensionRolesHelper.ImageFormat.jpg );
			Assert.AreEqual("00010101_000000_d.jpg", fileName);
		}

		[TestMethod]
		public void ParseFileName_FileNameBase()
		{
			var structure = "/yyyy/MM/yyyy_MM_dd/yyyyMMdd_HHmmss_{filenamebase}.ext";
			var importItem = new StructureService(new FakeIStorage(), structure);
			var fileName = importItem.ParseFileName(
				new DateTime(2020, 01, 01, 01, 01, 01), 
				"test", ExtensionRolesHelper.ImageFormat.jpg );
			
			Assert.AreEqual("20200101_010101_test.jpg", fileName);
		}

		[TestMethod]
		[ExpectedException(typeof(FieldAccessException))]
		public void ParseFileName_FieldAccessException_Null()
		{
			string structure = null;
			new StructureService(new FakeIStorage(), structure).ParseSubfolders(
				new DateTime(2020, 01, 01, 01, 01, 01));
			// ExpectedException
		}

		[TestMethod]
		public void ParseSubfolders_TrFolder_RealFs()
		{
			var structure = "/\\t*/yyyyMMdd_HHmmss_{filenamebase}.ext";
			var result = new StructureService(new StorageSubPathFilesystem(new AppSettings
			{
				StorageFolder = new CreateAnImage().BasePath
			}), structure).ParseSubfolders(
				new DateTime(2020, 01, 01, 01, 01, 01));
			
			Assert.AreEqual("/tr/",result);
		}
		
		[TestMethod]
		public void ParseSubfolders_Asterisk_Test_Folder()
		{
			var structure = "/\\t*/yyyyMMdd_HHmmss_{filenamebase}.ext";
			
			var storage = new FakeIStorage(
				new List<string>{"/", "/test","/something"},
				new List<string>());
			
			var result = new StructureService(storage,structure).ParseSubfolders(
				new DateTime(2020, 01, 01, 01, 01, 01));
			
			Assert.AreEqual("/test/",result);
		}

		[TestMethod]
		public void ParseSubfolders_ReturnNewChildFolder()
		{
			var storage = new FakeIStorage(
				new List<string>{"/2020","/2020/01"},
				new List<string>());
			
			var structure = "/yyyy/MM/yyyy_MM_dd*/yyyyMMdd_HHmmss_{filenamebase}.ext";
			
			var result = new StructureService(storage,structure).ParseSubfolders(
				new DateTime(2020, 01, 01, 01, 01, 01));
			
			Assert.AreEqual("/2020/01/2020_01_01/",result);
		}
		
		[TestMethod]
		public void ParseSubfolders_DefaultAsterisk()
		{
			var storage = new FakeIStorage(
				new List<string>{"/"},
				new List<string>());
			
			var structure = "/*/yyyyMMdd_HHmmss_{filenamebase}.ext";
			
			var result = new StructureService(storage,structure).ParseSubfolders(
				new DateTime(2020, 01, 01, 01, 01, 01));
			
			Assert.AreEqual("/default/",result);
		}
		
		[TestMethod]
		public void ParseSubfolders_ExistingAsterisk()
		{
			var storage = new FakeIStorage(
				new List<string>{"/", "/any"},
				new List<string>());
			
			var structure = "/*/yyyyMMdd_HHmmss_{filenamebase}.ext";
			
			var result = new StructureService(storage,structure).ParseSubfolders(
				new DateTime(2020, 01, 01, 01, 01, 01));
			
			Assert.AreEqual("/any/",result);
		}
		
		[TestMethod]
		public void ParseSubfolders_GetExistingFolder()
		{
			var storage = new FakeIStorage(
				new List<string>{"/", "/2020","/2020/01","/2020/01/2020_01_01 test","/2020/01/2020_01_01 test/ignore"},
				new List<string>());
			
			var structure = "/yyyy/MM/yyyy_MM_dd*/yyyyMMdd_HHmmss_{filenamebase}.ext";
			
			var result = new StructureService(storage,structure).ParseSubfolders(
				new DateTime(2020, 01, 01, 01, 01, 01));
			
			Assert.AreEqual("/2020/01/2020_01_01 test/",result);
		}
		
		[TestMethod]
		public void ParseSubfolders_GetExistingPreferSimpleName()
		{
			var storage = new FakeIStorage(
				new List<string>{"/", "/2020","/2020/01","/2020/01/2020_01_01", "/2020/01/2020_01_01 test"},
				new List<string>());
			
			var structure = "/yyyy/MM/yyyy_MM_dd*/yyyyMMdd_HHmmss_{filenamebase}.ext";
			
			var result = new StructureService(storage,structure).ParseSubfolders(
				new DateTime(2020, 01, 01, 01, 01, 01));
			
			Assert.AreEqual("/2020/01/2020_01_01/",result);
		}

		[TestMethod]
		public void ParseSubfolders_FileNameBaseOnFolder()
		{
			var storage = new FakeIStorage(new List<string>{"/"},
				new List<string>());
			
			var structure = "/{filenamebase}/file.ext";
			var result = new StructureService(storage,structure).ParseSubfolders(
				new DateTime(2020, 01, 01, 01, 01, 01),"test");
			Assert.AreEqual("/test/",result);
		}
		
		[TestMethod]
		public void ParseSubfolders_ExtensionWantedInFolderName()
		{
			var storage = new FakeIStorage(new List<string>{"/"},
				new List<string>());
			
			var structure = "/con\\ten\\t.ext/file.ext";
			var result = new StructureService(storage,structure).ParseSubfolders(
				new DateTime(2020, 01, 01, 01, 01, 01),"test");
			Assert.AreEqual("/content.unknown/",result);
		}

		[TestMethod]
		[ExpectedException(typeof(FieldAccessException))]
		public void ParseSubfolders_FieldAccessException_String()
		{
			var structure = "";
			new StructureService(new FakeIStorage(), structure).ParseSubfolders(
				new DateTime(2020, 01, 01, 01, 01, 01));
			// ExpectedException
		}
		
		[TestMethod]
		[ExpectedException(typeof(FieldAccessException))]
		public void ParseSubfolders_FieldAccessException_DotExt()
		{
			var structure = "/.ext";
			new StructureService(new FakeIStorage(), structure).ParseSubfolders(
				new DateTime(2020, 01, 01, 01, 01, 01));
			// ExpectedException
		}
		
		[TestMethod]
		[ExpectedException(typeof(FieldAccessException))]
		public void ParseSubfolders_FieldAccessException_DoesNotStartWithSlash()
		{
			var structure = "test/on";
			new StructureService(new FakeIStorage(), structure).ParseSubfolders(
				new DateTime(2020, 01, 01, 01, 01, 01));
			// ExpectedException
		}

	}
}
