using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using starsky.foundation.platform.JsonConverter;

namespace starskytest.starsky.foundation.platform.JsonConverter;

[TestClass]
public class EnumListConverterTests
{
	[TestMethod]
	[ExpectedException(typeof(JsonException))]
	public void No_StartArray()
	{
		// Arrange
		const string json = "{\"ValueTypes\":\"Value1\"}";
		var options = DefaultJsonSerializer.CamelCase;
		JsonSerializer.Deserialize<ValueTypeContainer>(json, options);
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
	[ExpectedException(typeof(JsonException), "Unknown enum value: InvalidValue")]
	public void TestYourEnumContainer_Deserialize_InvalidValue()
	{
		// Arrange
		const string json = "{\"ValueTypes\":[\"Value1\",\"InvalidValue\",\"Value2\"]}";

		// Act
		JsonSerializer.Deserialize<ValueTypeContainer>(json, DefaultJsonSerializer.CamelCase);

		// Assert
		// Should throw JsonException
	}

	[TestMethod]
	[ExpectedException(typeof(JsonException), "Unexpected end of JSON input")]
	public void TestYourEnumContainer_Deserialize_UnexpectedEnd()
	{
		// Arrange
		const string json = "{\"ValueTypes\":[\"Value1\",\"Value2\"";

		// Act
		JsonSerializer.Deserialize<ValueTypeContainer>(json, DefaultJsonSerializer.CamelCase);

		// Assert
		// Should throw JsonException
	}

	[TestMethod]
	[ExpectedException(typeof(JsonException))]
	public void Read_WhenTokenTypeIsNotStartArray_ThrowsJsonException()
	{
		// Arrange
		var reader = new Utf8JsonReader(Array.Empty<byte>());
		var converter =
			new EnumListConverter<ValueType>(); // Replace YourEnum with the actual enum type

		// Act & Assert
		converter.Read(ref reader, typeof(List<ValueType>), new JsonSerializerOptions());
	}

	[TestMethod]
	[ExpectedException(typeof(JsonException))]
	public void Read_WhenTokenTypeIsNotString_ThrowsJsonException()
	{
		// Arrange
		var reader = new Utf8JsonReader(new[] { ( byte )'[', ( byte )'1', ( byte )']' });
		var converter = new EnumListConverter<ValueType>();

		// Act & Assert
		converter.Read(ref reader, typeof(List<ValueType>), new JsonSerializerOptions());
	}


	public class ValueTypeContainer
	{
		[JsonConverter(typeof(EnumListConverter<ValueType>))]
		public List<ValueType> ValueTypes { get; set; } = [];
	}

	public enum ValueType
	{
		Value1,
		Value2,
		Value3
	}
}
