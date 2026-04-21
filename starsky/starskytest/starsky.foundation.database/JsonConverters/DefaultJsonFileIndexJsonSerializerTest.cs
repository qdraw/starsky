using System;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.JsonConverters;
using starsky.foundation.database.Models;

namespace starskytest.starsky.foundation.database.JsonConverters;

[TestClass]
public sealed class DefaultJsonFileIndexJsonSerializerTest
{
	[TestMethod]
	public void Serialize_WithIdConverter_IncludesId()
	{
		var item = new FileIndexItem
		{
			FileName = "test.jpg",
			ParentDirectory = "/",
			FileHash = "hash",
			// set Id even though model has [JsonIgnore]
			Id = 42
		};

		var json =
			JsonSerializer.Serialize(item, DefaultJsonFileIndexJsonSerializer.WithIdConverter);

		// The custom converter should include the id field
		Assert.IsTrue(json.Contains("\"id\":42") || json.Contains("\"id\": 42"), json);

		// Parse JSON and assert that a filename property exists (case-insensitive)
		using var doc = JsonDocument.Parse(json);
		var root = doc.RootElement;
		var hasFileName = false;
		foreach ( var prop in root.EnumerateObject() )
		{
			if ( string.Equals(prop.Name, "fileName", StringComparison.OrdinalIgnoreCase) ||
			     string.Equals(prop.Name, "FileName", StringComparison.OrdinalIgnoreCase) )
			{
				hasFileName = true;
				break;
			}
		}

		Assert.IsTrue(hasFileName, json);
	}

	[TestMethod]
	public void Serialize_ListWithOptions_IncludesIdForEach()
	{
		var items = new List<FileIndexItem>
		{
			new() { Id = 1, FileName = "a.jpg" }, new() { Id = 2, FileName = "b.jpg" }
		};

		var json =
			JsonSerializer.Serialize(items, DefaultJsonFileIndexJsonSerializer.WithIdConverter);

		Assert.Contains("\"id\":1", json, json);
		Assert.Contains("\"id\":2", json, json);
	}

	[TestMethod]
	public void Serialize_ToArrayWithOptions_IncludesIdForEach()
	{
		var items = new List<FileIndexItem>
		{
			new() { Id = 1, FileName = "a.jpg" }, new() { Id = 2, FileName = "b.jpg" }
		}.ToArray();

		var json =
			JsonSerializer.Serialize(items,
				DefaultJsonFileIndexJsonSerializer.WithIdConverter);

		Assert.Contains("\"id\":1", json, json);
		Assert.Contains("\"id\":2", json, json);
	}

	[TestMethod]
	public void Deserialize_List_NullJson_ReturnsNull()
	{
		const string json = "null";
		var result = JsonSerializer.Deserialize<List<FileIndexItem>>(json,
			DefaultJsonFileIndexJsonSerializer.WithIdConverter);
		Assert.IsNull(result);
	}

	[TestMethod]
	public void Deserialize_List_WrongToken_ThrowsJsonException()
	{
		const string json = "{}"; // not an array
		Assert.ThrowsExactly<JsonException>(() =>
			JsonSerializer.Deserialize<List<FileIndexItem>>(json,
				DefaultJsonFileIndexJsonSerializer.WithIdConverter));
	}

	[TestMethod]
	public void Deserialize_Array_NullJson_ReturnsNull()
	{
		const string json = "null";
		var result = JsonSerializer.Deserialize<FileIndexItem[]>(json,
			DefaultJsonFileIndexJsonSerializer.WithIdConverter);
		Assert.IsNull(result);
	}

	[TestMethod]
	public void Deserialize_Array_WrongToken_ThrowsJsonException()
	{
		const string json = "{}"; // not an array
		Assert.ThrowsExactly<JsonException>(() =>
			JsonSerializer.Deserialize<FileIndexItem[]>(json,
				DefaultJsonFileIndexJsonSerializer.WithIdConverter));
	}
}
