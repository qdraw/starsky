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
