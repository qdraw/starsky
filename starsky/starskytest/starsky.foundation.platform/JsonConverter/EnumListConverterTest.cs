using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.JsonConverter;

namespace starskytest.starsky.foundation.platform.JsonConverter;

[TestClass]
public class EnumListConverterTests
{
	public enum ValueType
	{
		Value1,
		Value2,
		Value3
	}

	[TestMethod]
	public void No_StartArray()
	{
		// Arrange
		const string json = "{\"ValueTypes\":\"Value1\"}";
		var options = DefaultJsonSerializer.CamelCase;

		// Act & Assert
		var ex = Assert.ThrowsException<JsonException>(() =>
		{
			JsonSerializer.Deserialize<ValueTypeContainer>(json, options);
		});

		// Additional assertions to verify the exception message, if necessary
		Assert.IsTrue(ex.Message.Contains("The JSON value could not be converted"));
	}

	[TestMethod]
	public void TestYourEnumContainer_Deserialize()
	{
		// Arrange
		const string json = "{\"ValueTypes\":[\"Value1\",\"Value2\",\"Value3\"]}";
		var options = DefaultJsonSerializer.CamelCase;

		// Act
		var container = JsonSerializer.Deserialize<ValueTypeContainer>(json, options);

		// Assert
		Assert.IsNotNull(container);
		Assert.IsNotNull(container.ValueTypes);
		Assert.AreEqual(3, container.ValueTypes.Count);
		Assert.AreEqual(ValueType.Value1, container.ValueTypes[0]);
		Assert.AreEqual(ValueType.Value2, container.ValueTypes[1]);
		Assert.AreEqual(ValueType.Value3, container.ValueTypes[2]);
	}

	[TestMethod]
	public void TestYourEnumContainer_Serialize()
	{
		// Arrange
		var container = new ValueTypeContainer
		{
			ValueTypes = [ValueType.Value1, ValueType.Value2]
		};

		// Act
		var json = JsonSerializer.Serialize(container, DefaultJsonSerializer.CamelCaseNoEnters);
		const string expectedJson = "{\"valueTypes\":[\"Value1\",\"Value2\"]}";

		// Assert
		Assert.AreEqual(expectedJson, json);
	}

	[TestMethod]
	public void TestYourEnumContainer_Deserialize_InvalidValue()
	{
		// Arrange
		const string json = "{\"ValueTypes\":[\"Value1\",\"InvalidValue\",\"Value2\"]}";
		var options = DefaultJsonSerializer.CamelCase;

		// Act & Assert
		var ex = Assert.ThrowsException<JsonException>(() =>
		{
			JsonSerializer.Deserialize<ValueTypeContainer>(json, options);
		});

		// Verify the exception message to ensure it contains the expected details
		Assert.IsTrue(ex.Message.Contains("Unknown enum value: InvalidValue"),
			"Exception message does not contain expected text.");
	}

	[TestMethod]
	public void TestYourEnumContainer_Deserialize_UnexpectedEnd()
	{
		// Arrange
		const string json = "{\"ValueTypes\":[\"Value1\",\"Value2\"";
		var options = DefaultJsonSerializer.CamelCase;

		// Act & Assert
		var ex = Assert.ThrowsException<JsonException>(() =>
		{
			JsonSerializer.Deserialize<ValueTypeContainer>(json, options);
		});

		// Verify the exception message to ensure it contains the expected text
		Assert.IsTrue(
			ex.Message.Contains("Expected depth to be zero at the end of the JSON payload."),
			"Exception message does not contain expected text.");
	}

	[TestMethod]
	public void Read_WhenTokenTypeIsNotStartArray_ThrowsJsonException()
	{
		// Arrange
		var converter =
			new EnumListConverter<ValueType>(); // Replace YourEnum with the actual enum type

		// Act & Assert
		Assert.ThrowsException<JsonException>(() =>
		{
			var reader = new Utf8JsonReader(Array.Empty<byte>());
			converter.Read(ref reader, typeof(List<ValueType>), new JsonSerializerOptions());
		});
	}

	[TestMethod]
	public void Read_WhenTokenTypeIsNotString_ThrowsJsonException()
	{
		// Arrange
		var converter = new EnumListConverter<ValueType>();

		// Act & Assert
		Assert.ThrowsException<JsonException>(() =>
		{
			var reader = new Utf8JsonReader(new[] { ( byte ) '[', ( byte ) '1', ( byte ) ']' });
			converter.Read(ref reader, typeof(List<ValueType>), new JsonSerializerOptions());
		});
	}

	[TestMethod]
	public void Read_ValidJsonArrayWithEnum()
	{
		// Arrange
		var converter = new EnumListConverter<ValueType>();

		const string json = "[\"Value1\", \"Value2\"]";
		var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
		reader.Read();

		// Act & Assert
		var result =
			converter.Read(ref reader, typeof(List<ValueType>), new JsonSerializerOptions());

		CollectionAssert.AreEqual(new List<ValueType> { ValueType.Value1, ValueType.Value2 },
			result);
	}

	[TestMethod]
	public void InvalidArrayWithNumber_ThrowJsonException()
	{
		// Arrange
		var converter = new EnumListConverter<ValueType>();

		// JSON array with a non-string token
		const string json = "[1]";

		// Act & Assert
		Assert.ThrowsException<JsonException>(() =>
		{
			var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
			reader.Read(); // Read the start of the array
			converter.Read(ref reader, typeof(List<ValueType>), new JsonSerializerOptions());
		});
	}

	public class ValueTypeContainer
	{
		[JsonConverter(typeof(EnumListConverter<ValueType>))]
		public List<ValueType> ValueTypes { get; set; } = [];
	}
}
