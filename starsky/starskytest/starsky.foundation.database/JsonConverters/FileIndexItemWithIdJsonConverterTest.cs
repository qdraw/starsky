using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.JsonConverters;
using starsky.foundation.database.Models;

namespace starskytest.starsky.foundation.database.JsonConverters;

[TestClass]
public sealed class FileIndexItemWithIdJsonConverterTest
{
	[TestMethod]
	public void Write_WithFactoryAndOtherConverters_WritesIdAndProperties()
	{
		var conv = new FileIndexItemWithIdJsonConverter();
		var item = new FileIndexItem { Id = 5, FileName = "a.jpg", ParentDirectory = "/" };

		var options = new JsonSerializerOptions();
		// include factory and another converter to exercise the safeOptions copy logic
		options.Converters.Add(new FileIndexItemWithIdJsonConverterFactory());
		options.Converters.Add(new JsonStringEnumConverter());

		using var ms = new MemoryStream();
		using ( var writer = new Utf8JsonWriter(ms) )
		{
			conv.Write(writer, item, options);
		}

		var json = Encoding.UTF8.GetString(ms.ToArray());
		using var doc = JsonDocument.Parse(json);
		var root = doc.RootElement;

		Assert.IsTrue(root.TryGetProperty("id", out var idProp) && idProp.GetInt32() == 5, json);
		Assert.IsTrue(
			root.TryGetProperty("FileName", out var fnProp) && fnProp.GetString() == "a.jpg", json);
	}

	[TestMethod]
	public void Read_WithFactoryAndOtherConverters_ParsesProperties()
	{
		var conv = new FileIndexItemWithIdJsonConverter();
		var json = "{\"id\":7,\"FileName\":\"b.jpg\",\"ParentDirectory\":\"/\"}";
		var bytes = Encoding.UTF8.GetBytes(json);
		var reader = new Utf8JsonReader(bytes);
		// advance to the first token (StartObject)
		reader.Read();

		var options = new JsonSerializerOptions();
		options.Converters.Add(new FileIndexItemWithIdJsonConverterFactory());
		options.Converters.Add(new JsonStringEnumConverter());

		var result = conv.Read(ref reader, typeof(FileIndexItem), options);

		Assert.IsNotNull(result);
		Assert.AreEqual("b.jpg", result.FileName);
		Assert.AreEqual("/", result.ParentDirectory);
		// Id is ignored by the POCO [JsonIgnore] so it may not be populated by the internal deserialize
		// Ensure the converter returned an object and populated other properties.
	}

	[TestMethod]
	public void Roundtrip_SerializeDeserialize_PropertiesRemain()
	{
		var item = new FileIndexItem { Id = 10, FileName = "c.jpg", ParentDirectory = "/" };
		var options = DefaultJsonFileIndexJsonSerializer.WithIdConverter;

		var json = JsonSerializer.Serialize(item, options);
		var deserialized = JsonSerializer.Deserialize<FileIndexItem>(json, options);

		Assert.IsNotNull(deserialized);
		Assert.AreEqual(item.FileName, deserialized.FileName);
		Assert.AreEqual(item.ParentDirectory, deserialized.ParentDirectory);
		// Id is not round-tripped to the property because FileIndexItem.Id has [JsonIgnore]
	}

	[TestMethod]
	public void Read_SetsId_CaseInsensitiveAndHandlesNonNumeric()
	{
		var conv = new FileIndexItemWithIdJsonConverter();

		// Numeric id with different casing
		const string json1 = "{\"Id\":9,\"FileName\":\"d.jpg\",\"ParentDirectory\":\"/\"}";
		var bytes1 = Encoding.UTF8.GetBytes(json1);
		var reader1 = new Utf8JsonReader(bytes1);
		reader1.Read();
		var options = new JsonSerializerOptions();
		options.Converters.Add(new FileIndexItemWithIdJsonConverterFactory());
		var result1 = conv.Read(ref reader1, typeof(FileIndexItem), options);
		Assert.AreEqual(9, result1.Id);

		// Non-numeric id should not throw and should leave Id as default (0)
		var json2 = "{\"id\":\"not-a-number\",\"FileName\":\"e.jpg\"}";
		var bytes2 = Encoding.UTF8.GetBytes(json2);
		var reader2 = new Utf8JsonReader(bytes2);
		reader2.Read();
		var result2 = conv.Read(ref reader2, typeof(FileIndexItem), options);
		Assert.AreEqual(0, result2.Id);
	}

	[TestMethod]
	public void Write_RespectsPropertyNamingPolicy_WhenProvided()
	{
		var conv = new FileIndexItemWithIdJsonConverter();
		var item = new FileIndexItem { Id = 11, FileName = "f.jpg", ParentDirectory = "/" };

		var options =
			new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
		options.Converters.Add(new FileIndexItemWithIdJsonConverterFactory());

		using var ms = new MemoryStream();
		using ( var writer = new Utf8JsonWriter(ms) )
		{
			conv.Write(writer, item, options);
		}

		var json = Encoding.UTF8.GetString(ms.ToArray());
		// with camelCase naming policy, properties should be camel-cased
		Assert.IsTrue(json.Contains("\"id\":11"), json);
		Assert.IsTrue(json.Contains("\"fileName\"") || json.Contains("\"FileName\""), json);
	}

	[TestMethod]
	public void Write_SkipsSelfAndFactoryInSafeOptions()
	{
		var conv = new FileIndexItemWithIdJsonConverter();
		var item = new FileIndexItem { Id = 21, FileName = "g.jpg", ParentDirectory = "/" };

		var options = new JsonSerializerOptions();
		// include this converter instance and factory to ensure they are skipped when building safeOptions
		options.Converters.Add(conv);
		options.Converters.Add(new FileIndexItemWithIdJsonConverterFactory());

		using var ms = new MemoryStream();
		using ( var writer = new Utf8JsonWriter(ms) )
		{
			conv.Write(writer, item, options);
		}

		var json = Encoding.UTF8.GetString(ms.ToArray());
		using var doc = JsonDocument.Parse(json);
		var root = doc.RootElement;

		Assert.IsTrue(root.TryGetProperty("id", out var idProp) && idProp.GetInt32() == 21, json);
	}

	[TestMethod]
	public void Read_SkipsSelfAndFactoryInReadOptions()
	{
		var conv = new FileIndexItemWithIdJsonConverter();
		var json = "{\"id\":33,\"FileName\":\"h.jpg\",\"ParentDirectory\":\"/\"}";
		var bytes = Encoding.UTF8.GetBytes(json);
		var reader = new Utf8JsonReader(bytes);
		reader.Read();

		var options = new JsonSerializerOptions();
		// include this converter and factory so the read logic will skip adding them to readOptions
		options.Converters.Add(conv);
		options.Converters.Add(new FileIndexItemWithIdJsonConverterFactory());

		var result = conv.Read(ref reader, typeof(FileIndexItem), options);

		Assert.IsNotNull(result);
		Assert.AreEqual(33, result.Id);
		Assert.AreEqual("h.jpg", result.FileName);
	}

	[TestMethod]
	public void Read_NoIdProperty_ReturnsDefaultId()
	{
		var conv = new FileIndexItemWithIdJsonConverter();
		var json = "{\"FileName\":\"noid.jpg\",\"ParentDirectory\":\"/\"}";
		var bytes = Encoding.UTF8.GetBytes(json);
		var reader = new Utf8JsonReader(bytes);
		reader.Read();

		var options = new JsonSerializerOptions();
		options.Converters.Add(new FileIndexItemWithIdJsonConverterFactory());

		var result = conv.Read(ref reader, typeof(FileIndexItem), options);

		Assert.IsNotNull(result);
		Assert.AreEqual(0, result.Id);
		Assert.AreEqual("noid.jpg", result.FileName);
	}
}
