using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Models;
using starsky.foundation.metaupdate.Interfaces;
using starsky.foundation.metaupdate.Models;
using starsky.foundation.metaupdate.Services;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.metaupdate.Services
{
	[TestClass]
	public sealed class MetaReplaceBackgroundJobHandlerTest
	{
		[TestMethod]
		public void JobType_CheckValue()
		{
			var handler = new MetaReplaceBackgroundJobHandler(new FakeIServiceScopeFactory());
			Assert.AreEqual("MetaReplace.v1", handler.JobType);
		}

		[TestMethod]
		public async Task ExecuteAsync_NullPayload_ThrowsArgumentException()
		{
			var handler = new MetaReplaceBackgroundJobHandler(new FakeIServiceScopeFactory());
			await Assert.ThrowsExactlyAsync<ArgumentException>(() => 
				handler.ExecuteAsync(null, CancellationToken.None));
		}

		[TestMethod]
		public async Task ExecuteAsync_WhitespacePayload_ThrowsArgumentException()
		{
			var handler = new MetaReplaceBackgroundJobHandler(new FakeIServiceScopeFactory());
			await Assert.ThrowsExactlyAsync<ArgumentException>(() => 
				handler.ExecuteAsync(" ", CancellationToken.None));
		}

		[TestMethod]
		public async Task ExecuteAsync_InvalidJson_ThrowsException()
		{
			var handler = new MetaReplaceBackgroundJobHandler(new FakeIServiceScopeFactory());
			// Depending on what JsonSerializer throws, it could be JsonException
			await Assert.ThrowsExactlyAsync<JsonException>(() => 
				handler.ExecuteAsync("{ invalid }", CancellationToken.None));
		}

		[TestMethod]
		public async Task ExecuteAsync_ValidPayload_CallsUpdateAsync()
		{
			var fakeMetaUpdateService = new FakeIMetaUpdateService();
			var scopeFactory = new FakeIServiceScopeFactory(null, (services) =>
			{
				services.AddSingleton<IMetaUpdateService>(fakeMetaUpdateService);
			});
			
			var handler = new MetaReplaceBackgroundJobHandler(scopeFactory);

			var payload = new MetaReplaceBackgroundPayload
			{
				ChangedFileIndexItemName = new Dictionary<string, List<string>>
				{
					{ "/test", new List<string> { "Tags" } }
				},
				ResultsOkOrDeleteList = new List<FileIndexItem>
				{
					new FileIndexItem("/test") { Status = FileIndexItem.ExifStatus.Ok }
				},
				Collections = true
			};

			var jsonPayload = JsonSerializer.Serialize(payload);
			await handler.ExecuteAsync(jsonPayload, CancellationToken.None);

			Assert.AreEqual(1, fakeMetaUpdateService.ChangedFileIndexItemNameContent.Count);
			var actualChanged = fakeMetaUpdateService.ChangedFileIndexItemNameContent.First();
			Assert.IsTrue(actualChanged.ContainsKey("/test"));
			Assert.AreEqual("Tags", actualChanged["/test"].First());
		}
	}
}
