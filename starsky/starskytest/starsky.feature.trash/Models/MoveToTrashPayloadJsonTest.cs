using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.trash.Models;
using starsky.foundation.database.JsonConverters;
using starsky.foundation.database.Models;

namespace starskytest.starsky.feature.trash.Models;

[TestClass]
public sealed class MoveToTrashPayloadJsonTest
{
	[TestMethod]
	public void Serialize_MoveToTrashPayload_IncludesIds()
	{
		var payload = new MoveToTrashPayload
		{
			MoveToTrashList =
			[
				new FileIndexItem { Id = 10, FileName = "a.jpg" },
				new FileIndexItem { Id = 20, FileName = "b.jpg" }
			],
			FileIndexResultsList = [new FileIndexItem { Id = 30, FileName = "c.jpg" }],
			InputModel = new FileIndexItem { Id = 40, FileName = "d.jpg" }
		};

		// Use the serializer options that register the converter as well
		var json =
			JsonSerializer.Serialize(payload, DefaultJsonFileIndexJsonSerializer.WithIdConverter);

		Assert.Contains("\"id\":10", json, json);
		Assert.Contains("\"id\":20", json, json);
		Assert.Contains("\"id\":30", json, json);
		Assert.Contains("\"id\":40", json, json);
	}
}
