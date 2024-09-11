using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.JsonConverter;

namespace starskytest.starsky.foundation.platform.JsonConverter;

[TestClass]
[SuppressMessage("Performance", "CA1869:Cache and reuse \'JsonSerializerOptions\' instances")]
public sealed class JsonBoolQuotedConverterTest
{
	[TestMethod]
	public void Write_Serialize()
	{
		var json = JsonSerializer.Serialize(
			new Dictionary<string, bool> { { "key", true } },
			new JsonSerializerOptions { Converters = { new JsonBoolQuotedConverter() } });

		Assert.AreEqual("{\"key\":\"true\"}", json);
	}

	[TestMethod]
	public void Read_DeserializeQuotedTrue()
	{
		var output = JsonSerializer.Deserialize<KeyExample>(
			"{\"Key\":\"true\"}",
			new JsonSerializerOptions { Converters = { new JsonBoolQuotedConverter() } }
		);

		Assert.IsTrue(output?.Key);
	}

	[TestMethod]
	public void Read_DeserializeQuotedFalse()
	{
		var output = JsonSerializer.Deserialize<KeyExample>(
			"{\"Key\":\"false\"}",
			new JsonSerializerOptions { Converters = { new JsonBoolQuotedConverter() } }
		);

		Assert.IsFalse(output?.Key);
	}

	[TestMethod]
	public void Read_DeserializeNonQuoted()
	{
		var output = JsonSerializer.Deserialize<KeyExample>(
			"{\"Key\":true}",
			new JsonSerializerOptions { Converters = { new JsonBoolQuotedConverter() } }
		);

		Assert.IsTrue(output?.Key);
	}

	[TestMethod]
	public void Read_DeserializeNonValidType()
	{
		// Arrange
		const string json = "{\"Key\":1}";
		var options = new JsonSerializerOptions { Converters = { new JsonBoolQuotedConverter() } };

		// Act & Assert
		var ex = Assert.ThrowsException<JsonException>(() =>
		{
			JsonSerializer.Deserialize<KeyExample>(json, options);
		});

		// Optionally verify the exception message if needed
		Assert.IsTrue(ex.Message.Contains("JSON value could not be converted"),
			"Exception message does not contain expected text.");
	}


	[TestMethod]
	public void Read_Deserialize_Null()
	{
		var output = JsonSerializer.Deserialize<KeyExample>(
			"{\"Key\":null}",
			new JsonSerializerOptions { Converters = { new JsonBoolQuotedConverter() } }
		);

		// Expect exception
		Assert.IsFalse(output?.Key);
	}

	[SuppressMessage("Usage", "S1144:Unused private types or members should be removed")]
	[SuppressMessage("Usage", "S3459:Unassigned members should be removed")]
	[SuppressMessage("ReSharper", "PropertyCanBeMadeInitOnly.Local")]
	private class KeyExample
	{
		public bool Key { get; set; }
	}
}
