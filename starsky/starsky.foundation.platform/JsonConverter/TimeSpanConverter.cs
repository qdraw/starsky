using System.Diagnostics.CodeAnalysis;
using System.Globalization;
// source: https://github.com/Macross-Software/core/blob/9da46ac4aff13a16c639f9a354214798cfc3a38b/
// ClassLibraries/Macross.Json.Extensions/Code/System.Text.Json.Serialization/JsonTimeSpanConverter.cs

// ReSharper disable once CheckNamespace
namespace System.Text.Json.Serialization
{
	/// <summary>
	/// <see cref="JsonConverterFactory"/> to convert <see cref="TimeSpan"/> to
	/// and from strings. Supports <see cref="Nullable{TimeSpan}"/>.
	/// </summary>
	/// <remarks>
	/// TimeSpans are transposed using the constant ("c") format specifier: [-][d.]hh:mm:ss[.fffffff].
	/// </remarks>
	[SuppressMessage("Usage", "CA1062:Validate arguments of public methods")]
	public class JsonTimeSpanConverter : JsonConverterFactory
	{
		/// <inheritdoc/>
		public override bool CanConvert(Type typeToConvert)
		{
			// Don't perform a typeToConvert == null check for performance. Trust our callers will be nice.
			return typeToConvert == typeof(TimeSpan)
				|| ( typeToConvert.IsGenericType && IsNullableTimeSpan(typeToConvert) );
		}

		/// <inheritdoc/>
		[SuppressMessage("Usage", "CA1062:Validate arguments of public methods")]
		public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
		{
			// Don't perform a typeToConvert == null check for performance. Trust our callers will be nice.
			return typeToConvert.IsGenericType
				? new JsonNullableTimeSpanConverter()
				: new JsonStandardTimeSpanConverter();
		}

		private static bool IsNullableTimeSpan(Type typeToConvert)
		{
			// Type?
			var underlyingType = Nullable.GetUnderlyingType(typeToConvert);
			return underlyingType != null && underlyingType == typeof(TimeSpan);
		}

		internal class JsonStandardTimeSpanConverter : JsonConverter<TimeSpan>
		{
			/// <inheritdoc/>
			public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
			{
				return reader.TokenType != JsonTokenType.String
					? throw new JsonException()
					: TimeSpan.ParseExact(reader.GetString()!, "c", CultureInfo.InvariantCulture);
			}

			/// <inheritdoc/>
			public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
				=> writer.WriteStringValue(value.ToString("c", CultureInfo.InvariantCulture));
		}

		internal class JsonNullableTimeSpanConverter : JsonConverter<TimeSpan?>
		{
			/// <inheritdoc/>
			public override TimeSpan? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
			{
				return reader.TokenType != JsonTokenType.String
					? throw new JsonException()
					: TimeSpan.ParseExact(reader.GetString()!, "c", CultureInfo.InvariantCulture);
			}

			/// <inheritdoc/>
			public override void Write(Utf8JsonWriter writer, TimeSpan? value, JsonSerializerOptions options)
				=> writer.WriteStringValue(value?.ToString("c", CultureInfo.InvariantCulture));
		}
	}
}
