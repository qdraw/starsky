using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace starskytest.starsky.foundation.platform.JsonConverter
{
	/// <summary>
	/// @see: https://github.com/Macross-Software/core/blob/9da46ac4aff13a16c639f9a354214798cfc3a38b/
	/// ClassLibraries/Macross.Json.Extensions/Test/JsonTimeSpanConverterTests.cs
	/// </summary>
	[TestClass]
	public class JsonTimeSpanConverterTests
	{
		[TestMethod]
		public void TimeSpanSerializationTest()
		{
			var json = JsonSerializer.Serialize(
				new TestClass
				{
					TimeSpan = new TimeSpan(1, 2, 3)
				});

			Assert.AreEqual(@"{""TimeSpan"":""01:02:03""}", json);
		}

		[TestMethod]
		public void NullableTimeSpanSerializationTest()
		{
			var json = JsonSerializer.Serialize(
				new NullableTestClass
				{
					TimeSpan = new TimeSpan(1, 2, 3)
				});

			Assert.AreEqual(@"{""TimeSpan"":""01:02:03""}", json);

			json = JsonSerializer.Serialize(new NullableTestClass());

			Assert.AreEqual(@"{""TimeSpan"":null}", json);
		}

		[TestMethod]
		public void TimeSpanSerializationUsingOptionsTest()
		{
			JsonSerializerOptions options = new JsonSerializerOptions();
			options.Converters.Add(new JsonTimeSpanConverter());

			string json = JsonSerializer.Serialize(new TimeSpan(1, 2, 3), options);

			Assert.AreEqual(@"""01:02:03""", json);

			TimeSpan? nullableTimeSpan = null;

			// ReSharper disable once ExpressionIsAlwaysNull
			json = JsonSerializer.Serialize(nullableTimeSpan, options);

			Assert.AreEqual(@"null", json);
		}

		[TestMethod]
		public void TimeSpanDeserializationTest()
		{
			var actual = JsonSerializer.Deserialize<TestClass>(@"{""TimeSpan"":""01:02:03""}");

			Assert.IsNotNull(actual);
			Assert.AreEqual(new TimeSpan(1, 2, 3), actual.TimeSpan);
		}

		[ExpectedException(typeof(JsonException))]
		[TestMethod]
		public void TimeSpanInvalidDeserializationTest() => JsonSerializer.Deserialize<
			TestClass>(@"{""TimeSpan"":null}");

		[TestMethod]
		public void NullableTimeSpanDeserializationTest()
		{
			NullableTestClass actual = JsonSerializer.Deserialize<
				NullableTestClass>(@"{""TimeSpan"":""01:02:03""}");

			Assert.IsNotNull(actual);
			Assert.IsTrue(actual.TimeSpan.HasValue);
			Assert.AreEqual(new TimeSpan(1, 2, 3), actual.TimeSpan);

			actual = JsonSerializer.Deserialize<NullableTestClass>(@"{""TimeSpan"":null}");

			Assert.IsNotNull(actual);
			Assert.IsFalse(actual.TimeSpan.HasValue);
		}

		[TestMethod]
		public void TimeSpanDeserializationUsingOptionsTest()
		{
			JsonSerializerOptions options = new JsonSerializerOptions();
			options.Converters.Add(new JsonTimeSpanConverter());

			TimeSpan? actual = JsonSerializer.Deserialize<TimeSpan>(@"""01:02:03""", options);

			Assert.IsTrue(actual.HasValue);
			Assert.AreEqual(new TimeSpan(1, 2, 3), actual);

			actual = JsonSerializer.Deserialize<TimeSpan?>(@"null", options);

			Assert.IsFalse(actual.HasValue);
		}

		[ExpectedException(typeof(JsonException))]
		[TestMethod]
		public void NullableTimeSpanInvalidDeserializationTest() => JsonSerializer.Deserialize<
			NullableTestClass>(@"{""TimeSpan"":1}");

		[ExpectedException(typeof(JsonException))]
		[TestMethod]
		public void TimeSpanInvalidDeserializationUsingOptionsTest()
		{
			JsonSerializerOptions options = new JsonSerializerOptions();
			options.Converters.Add(new JsonTimeSpanConverter());

			JsonSerializer.Deserialize<TimeSpan>(@"null", options);
		}

		private class TestClass
		{
			[JsonConverter(typeof(JsonTimeSpanConverter))]
			public TimeSpan TimeSpan { get; set; }
		}

		private class NullableTestClass
		{
			[JsonConverter(typeof(JsonTimeSpanConverter))]
			public TimeSpan? TimeSpan { get; set; }
		}
	}
}
