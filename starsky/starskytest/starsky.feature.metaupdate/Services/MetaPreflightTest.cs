using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.metaupdate.Services;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Models;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.metaupdate.Services
{
	[TestClass]
	public sealed class MetaPreflightTest
	{
		private static readonly string[] TestJpgArray = new[] {"/test.jpg"};
		private static readonly string[] OnlyTestWordArray = new[] {"test"};
		private static readonly string[] ReadonlyFolderTestJpgArray = new[] {"/readonly/test.jpg"};
		private static readonly string[] DeletedJpgArray = new[] {"/deleted.jpg"};

		[TestMethod]
		public async Task Preflight_Collections_Enabled()
		{
			var metaPreflight = new MetaPreflight(new FakeIQuery(new List<FileIndexItem>
				{
					new FileIndexItem("/test.jpg"),
					new FileIndexItem("/test.dng"),
				}), 
				new AppSettings(), new FakeSelectorStorage(
					new FakeIStorage(new List<string>(), 
						new List<string>{"/test.jpg", "/test.dng"}, 
						new []{CreateAnImage.Bytes.ToArray(), CreateAnImage.Bytes.ToArray()}))
				,new FakeIWebLogger());
			
			var result = await metaPreflight.PreflightAsync(
				new FileIndexItem("/test.jpg"),
				TestJpgArray.ToList(), true, true, 0);

			Assert.AreEqual(2, result.fileIndexResultsList.Count);
			Assert.AreEqual(FileIndexItem.ExifStatus.Ok, 
				result.fileIndexResultsList[0].Status);
			Assert.AreEqual(FileIndexItem.ExifStatus.Ok, 
				result.fileIndexResultsList[1].Status);
		}
		
		[TestMethod]
		public async Task Preflight_Collections_Disabled()
		{
			var metaPreflight = new MetaPreflight(new FakeIQuery(new List<FileIndexItem>
				{
					new FileIndexItem("/test.jpg"),
					new FileIndexItem("/test.dng"),
				}), 
				new AppSettings(), new FakeSelectorStorage(
					new FakeIStorage(new List<string>(), 
						new List<string>{"/test.jpg", "/test.dng"}, 
						new []{CreateAnImage.Bytes.ToArray(), CreateAnImage.Bytes.ToArray()}))
				,new FakeIWebLogger());
			
			var result = await metaPreflight.PreflightAsync(
				new FileIndexItem("/test.jpg"), 
				TestJpgArray.ToList(), true, false, 0);

			Assert.AreEqual(1, result.fileIndexResultsList.Count);
			Assert.AreEqual(FileIndexItem.ExifStatus.Ok, 
				result.fileIndexResultsList[0].Status);
		}
		
		[TestMethod]
		public async Task Preflight_InvalidLatLong()
		{
			var metaPreflight = new MetaPreflight(new FakeIQuery(new List<FileIndexItem>
				{
					new FileIndexItem("/test.jpg") {Latitude = 92043, Longitude = 38294923},
				}), 
				new AppSettings(), new FakeSelectorStorage(
					new FakeIStorage(new List<string>(), 
						new List<string>{"/test.jpg"}, 
						new []{CreateAnImage.Bytes.ToArray(), CreateAnImage.Bytes.ToArray()}))
				,new FakeIWebLogger());
			
			var result = await metaPreflight.PreflightAsync(
				new FileIndexItem("/test.jpg"), 
				TestJpgArray.ToList(), true, true, 0);

			Assert.AreEqual(1, result.fileIndexResultsList.Count);
			Assert.AreEqual(FileIndexItem.ExifStatus.OperationNotSupported, 
				result.fileIndexResultsList[0].Status);
		}
		
		[TestMethod]
		public async Task Preflight_ValidLatLong()
		{
			// 51.34963/5.46038 = valkenswaard

			var metaPreflight = new MetaPreflight(new FakeIQuery(new List<FileIndexItem>
				{
					new FileIndexItem("/test.jpg") {Latitude = 51.34963, Longitude = 5.46038},
				}), 
				new AppSettings(), new FakeSelectorStorage(
					new FakeIStorage(new List<string>(), 
						new List<string>{"/test.jpg"}, 
						new []{CreateAnImage.Bytes.ToArray(), CreateAnImage.Bytes.ToArray()}))
				,new FakeIWebLogger());
			
			var result = await metaPreflight.PreflightAsync(
				new FileIndexItem("/test.jpg"), 
				TestJpgArray.ToList(),  true, true, 0);

			Assert.AreEqual(1, result.fileIndexResultsList.Count);
			Assert.AreEqual(FileIndexItem.ExifStatus.Ok, 
				result.fileIndexResultsList[0].Status);
		}
		
		[TestMethod]
		[ExpectedException(typeof(MissingFieldException))]
		public void UpdateServiceTest_CompareAllLabelsAndRotation_NullMissingFieldException()
		{
			MetaPreflight.
				CompareAllLabelsAndRotation(null!, null!,
					null!, false, 0);
			// ==>> MissingFieldException
		}
		
		
		[TestMethod]
		public void UpdateServiceTest_CompareAllLabelsAndRotation_AppendIsFalse()
		{
			var changedFileIndexItemName = new Dictionary<string, List<string>>();
			
			var collectionsDetailView = new DetailView
			{
				FileIndexItem = new FileIndexItem
				{
					Status = FileIndexItem.ExifStatus.Ok,
					Tags = "initial Value",
					FileName = "test.jpg",
					ParentDirectory = "/",
					Orientation = FileIndexItem.Rotation.Horizontal
				}
			};

			var statusModel = new FileIndexItem
			{
				Status = FileIndexItem.ExifStatus.Ok,
				Tags = "updated Value",
				FileName = "test.jpg",
				ParentDirectory = "/"
			};
			
			// Check for compare values
			MetaPreflight
				.CompareAllLabelsAndRotation(changedFileIndexItemName, collectionsDetailView.FileIndexItem,
					statusModel, false, 0);
			
			// Check how that changedFileIndexItemName works
			Assert.AreEqual(1,changedFileIndexItemName["/test.jpg"].Count);
			Assert.AreEqual("tags",changedFileIndexItemName["/test.jpg"].FirstOrDefault());
			
			// Check for value
			Assert.AreEqual("updated Value", collectionsDetailView.FileIndexItem.Tags);
			Assert.AreEqual(FileIndexItem.Rotation.Horizontal, 
				collectionsDetailView.FileIndexItem.Orientation);
		}

		[TestMethod]
		public void Update_should_ignore_capital_compare()
		{
			var changedFileIndexItemName = new Dictionary<string, List<string>>
			{
				{ "/test.jpg", new List<string>() }
			};
			
			var collectionsDetailView = new DetailView
			{
				FileIndexItem = new FileIndexItem
				{
					Status = FileIndexItem.ExifStatus.Ok,
					Tags = "Value",
					FileName = "test.jpg",
					ParentDirectory = "/",
					Orientation = FileIndexItem.Rotation.Horizontal
				}
			};

			var statusModel = new FileIndexItem
			{
				Status = FileIndexItem.ExifStatus.Ok,
				Tags = "VALUE", // <-- capitals that's the diff
				FileName = "test.jpg",
				ParentDirectory = "/"
			};
			
			// Check for compare values
			MetaPreflight
				.CompareAllLabelsAndRotation(changedFileIndexItemName, collectionsDetailView.FileIndexItem,
					statusModel, false, 0);
			
			Assert.AreEqual(0,changedFileIndexItemName["/test.jpg"].Count);
		}
		
		[TestMethod]
		public void UpdateServiceTest_ShouldOverwrite()
		{
			var changedFileIndexItemName = new Dictionary<string, List<string>>
			{
				{ "/test.jpg", new List<string>() }
			};
			
			var collectionsDetailView = new DetailView
			{
				FileIndexItem = new FileIndexItem
				{
					Status = FileIndexItem.ExifStatus.Ok,
					Tags = "initial Value",
					FileName = "test.jpg",
					ParentDirectory = "/",
					Orientation = FileIndexItem.Rotation.Horizontal
				}
			};

			var statusModel = new FileIndexItem
			{
				Status = FileIndexItem.ExifStatus.Ok,
				Tags = "updated Value",
				FileName = "test.jpg",
				ParentDirectory = "/"
			};
			
			// Check for compare values
			MetaPreflight
				.CompareAllLabelsAndRotation(changedFileIndexItemName, collectionsDetailView.FileIndexItem,
					statusModel, false, 0);
			
			Assert.AreEqual("tags",changedFileIndexItemName["/test.jpg"][0]);
		}
	
		[TestMethod]
		public void UpdateServiceTest_CompareAllLabelsAndRotation_Rotate270Cw()
		{
			var changedFileIndexItemName = new Dictionary<string, List<string>>();
			
			var collectionsDetailView = new DetailView
			{
				FileIndexItem = new FileIndexItem
				{
					Status = FileIndexItem.ExifStatus.Ok,
					Tags = "initial Value",
					FileName = "test.jpg",
					ParentDirectory = "/"
				}
			};
			
			// Rotate right; check if values are the same
			MetaPreflight
				.CompareAllLabelsAndRotation(changedFileIndexItemName, collectionsDetailView.FileIndexItem, 
					collectionsDetailView.FileIndexItem, 
					false, -1);
			
			// Check for value
			Assert.AreEqual(FileIndexItem.Rotation.Rotate270Cw, 
				collectionsDetailView.FileIndexItem.Orientation);
		}

		[TestMethod]
		public async Task Preflight_NotFoundNotInIndex()
		{
			var metaPreflight = new MetaPreflight(new FakeIQuery(), new AppSettings(), 
				new FakeSelectorStorage(),new FakeIWebLogger());
			var result = await metaPreflight.PreflightAsync(
				new FileIndexItem("test"),
OnlyTestWordArray.ToList(), true, true, 0);
			
			Assert.AreEqual(FileIndexItem.ExifStatus.NotFoundNotInIndex, 
				result.fileIndexResultsList.FirstOrDefault()?.Status);
		}
		
		[TestMethod]
		public async Task Preflight_ReadOnly()
		{
			var metaPreflight = new MetaPreflight(new FakeIQuery(new List<FileIndexItem>
				{
					new FileIndexItem("/readonly/test.jpg")
				}), 
				new AppSettings{ ReadOnlyFolders = new List<string>{"readonly"}}, new FakeSelectorStorage(
					new FakeIStorage(new List<string>(), 
						new List<string>{"/readonly/test.jpg"}, 
						new []{CreateAnImage.Bytes.ToArray(), })),new FakeIWebLogger());
			
			var result = await metaPreflight.PreflightAsync(
				new FileIndexItem("/readonly/test.jpg"), 
				ReadonlyFolderTestJpgArray.ToList(), true, true, 0);
			
			Assert.AreEqual(FileIndexItem.ExifStatus.ReadOnly, 
				result.fileIndexResultsList.FirstOrDefault()?.Status);
			Assert.AreEqual("", result.fileIndexResultsList.FirstOrDefault()?.Tags);
		}
		
		[TestMethod]
		public async Task Preflight_Deleted()
		{
			var metaPreflight = new MetaPreflight(new FakeIQuery(new List<FileIndexItem>
				{
					new FileIndexItem("/deleted.jpg") {Tags = TrashKeyword.TrashKeywordString}
				}), 
				new AppSettings(), new FakeSelectorStorage(
					new FakeIStorage(new List<string>(), 
						new List<string>{"/deleted.jpg"}, 
						new []{CreateAnImage.Bytes.ToArray(), })),new FakeIWebLogger());
			
			var result = await metaPreflight.PreflightAsync(
				new FileIndexItem("/deleted.jpg"),
				DeletedJpgArray.ToList(), 
				true, true, 0);
			
			Assert.AreEqual(FileIndexItem.ExifStatus.Deleted, 
				result.fileIndexResultsList.FirstOrDefault()?.Status);
			Assert.AreEqual(TrashKeyword.TrashKeywordString, result.fileIndexResultsList.FirstOrDefault()?.Tags);
		}

		[TestMethod]
		public void RotationCompare_DoNotRotate()
		{
			var compareList = new List<string>();

			var rotationCompare = MetaPreflight.RotationCompare(0, 
				new FileIndexItem("/test.jpg"){Orientation = FileIndexItem.Rotation.Horizontal},
				compareList);
			Assert.AreEqual(FileIndexItem.Rotation.Horizontal, rotationCompare.Orientation);
			Assert.IsTrue(compareList.Count == 0);
		}
		
		[TestMethod]
		public void RotationCompare_Plus1()
		{
			var compareList = new List<string>();
			var rotationCompare = MetaPreflight.RotationCompare(1, 
				new FileIndexItem("/test.jpg"){Orientation = FileIndexItem.Rotation.Horizontal},
				compareList);
			Assert.AreEqual(FileIndexItem.Rotation.Rotate90Cw, rotationCompare.Orientation);
			Assert.AreEqual(nameof(FileIndexItem.Orientation).ToLowerInvariant(),
				compareList.FirstOrDefault());
		}
		
		[TestMethod]
		public void RotationCompare_Minus1()
		{
			var compareList = new List<string>();
			var rotationCompare = MetaPreflight.RotationCompare(-1, 
				new FileIndexItem("/test.jpg"){Orientation = FileIndexItem.Rotation.Horizontal},
				compareList);
			Assert.AreEqual(FileIndexItem.Rotation.Rotate270Cw,
				rotationCompare.Orientation);
			Assert.AreEqual(compareList.FirstOrDefault(),
				nameof(FileIndexItem.Orientation).ToLowerInvariant());
		}
		
		[TestMethod]
		public async Task Preflight_NotFoundSourceMissing()
		{
			var metaPreflight = new MetaPreflight(new FakeIQuery(
					new List<FileIndexItem>{new FileIndexItem("/test.jpg")}), new AppSettings(), 
				new FakeSelectorStorage(),new FakeIWebLogger());
			
			var result = await metaPreflight.PreflightAsync(
				new FileIndexItem("/test.jpg"),
				TestJpgArray.ToList(), true, true, 0);
			
			Assert.AreEqual(FileIndexItem.ExifStatus.NotFoundSourceMissing, 
				result.fileIndexResultsList.FirstOrDefault()?.Status);
		}


	}
}
