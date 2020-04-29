using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
		public void GetSubPaths_ReturnNewChildFolder()
		{
			var storage = new FakeIStorage(
				new List<string>{"/2020","/2020/01"},
				new List<string>());
			
			var structure = "/yyyy/MM/yyyy_MM_dd*/yyyyMMdd_HHmmss_{filenamebase}.ext";
			
			var result = new StructureService(storage,structure).GetSubPaths(
				new DateTime(2020, 01, 01, 01, 01, 01));
			
			Assert.AreEqual("/2020/01/2020_01_01/",result);
		}
		
		[TestMethod]
		public void GetSubPaths_DefaultAsterisk()
		{
			var storage = new FakeIStorage(
				new List<string>{"/"},
				new List<string>());
			
			var structure = "/*/yyyyMMdd_HHmmss_{filenamebase}.ext";
			
			var result = new StructureService(storage,structure).GetSubPaths(
				new DateTime(2020, 01, 01, 01, 01, 01));
			
			Assert.AreEqual("/default/",result);
		}
		
		[TestMethod]
		public void GetSubPaths_ExistingAsterisk()
		{
			var storage = new FakeIStorage(
				new List<string>{"/", "/any"},
				new List<string>());
			
			var structure = "/*/yyyyMMdd_HHmmss_{filenamebase}.ext";
			
			var result = new StructureService(storage,structure).GetSubPaths(
				new DateTime(2020, 01, 01, 01, 01, 01));
			
			Assert.AreEqual("/any/",result);
		}
		
		[TestMethod]
		public void GetSubPaths_GetExistingFolder()
		{
			var storage = new FakeIStorage(
				new List<string>{"/", "/2020","/2020/01","/2020/01/2020_01_01 test","/2020/01/2020_01_01 test/ignore"},
				new List<string>());
			
			var structure = "/yyyy/MM/yyyy_MM_dd*/yyyyMMdd_HHmmss_{filenamebase}.ext";
			
			var result = new StructureService(storage,structure).GetSubPaths(
				new DateTime(2020, 01, 01, 01, 01, 01));
			
			Assert.AreEqual("/2020/01/2020_01_01 test/",result);
		}
		
		[TestMethod]
		public void GetSubPaths_GetExistingPreferSimpleName()
		{
			var storage = new FakeIStorage(
				new List<string>{"/", "/2020","/2020/01","/2020/01/2020_01_01", "/2020/01/2020_01_01 test"},
				new List<string>());
			
			var structure = "/yyyy/MM/yyyy_MM_dd*/yyyyMMdd_HHmmss_{filenamebase}.ext";
			
			var result = new StructureService(storage,structure).GetSubPaths(
				new DateTime(2020, 01, 01, 01, 01, 01));
			
			Assert.AreEqual("/2020/01/2020_01_01/",result);
		}

		[TestMethod]
		public void GetSubPaths_RealFs()
		{
			var structure = "/\\t*/yyyyMMdd_HHmmss_{filenamebase}.ext";
			var result = new StructureService(new StorageSubPathFilesystem(new AppSettings
			{
				StorageFolder = new CreateAnImage().BasePath
			}), structure).GetSubPaths(
				new DateTime(2020, 01, 01, 01, 01, 01));
			
			Assert.AreEqual("/tr/",result);

		}

	}
}
