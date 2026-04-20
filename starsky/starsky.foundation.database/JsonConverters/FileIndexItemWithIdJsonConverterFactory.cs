using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using starsky.foundation.database.Models;

namespace starsky.foundation.database.JsonConverters;

/// <summary>
///     JsonConverterFactory that provides converters for FileIndexItem and common collections of
///     FileIndexItem
///     when the attribute is applied to either a single item or a list/array property.
/// </summary>
public sealed class FileIndexItemWithIdJsonConverterFactory : JsonConverterFactory
{
	public override bool CanConvert(Type typeToConvert)
	{
		if ( typeToConvert == typeof(FileIndexItem) )
		{
			return true;
		}

		if ( typeToConvert.IsArray && typeToConvert.GetElementType() == typeof(FileIndexItem) )
		{
			return true;
		}

		if ( !typeToConvert.IsGenericType )
		{
			return false;
		}

		var genDef = typeToConvert.GetGenericTypeDefinition();
		var arg = typeToConvert.GetGenericArguments()[0];
		if ( arg == typeof(FileIndexItem) &&
		     ( genDef == typeof(List<>) || genDef == typeof(IList<>) ||
		       genDef == typeof(IEnumerable<>) ) )
		{
			return true;
		}

		return false;
	}

	public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
	{
		if ( typeToConvert == typeof(FileIndexItem) )
		{
			return new FileIndexItemWithIdJsonConverter();
		}

		if ( typeToConvert.IsArray && typeToConvert.GetElementType() == typeof(FileIndexItem) )
		{
			return new FileIndexItemArrayConverter();
		}

		if ( typeToConvert.IsGenericType )
		{
			var genDef = typeToConvert.GetGenericTypeDefinition();
			var arg = typeToConvert.GetGenericArguments()[0];
			if ( ( arg == typeof(FileIndexItem) && genDef == typeof(List<>) ) ||
			     ( arg == typeof(FileIndexItem) && genDef == typeof(IEnumerable<>) ) ||
			     ( arg == typeof(FileIndexItem) && genDef == typeof(IList<>) ) )
			{
				return new ListFileIndexItemConverter();
			}
		}

		throw new NotSupportedException($"Cannot create converter for {typeToConvert}");
	}

	private sealed class ListFileIndexItemConverter : JsonConverter<List<FileIndexItem>>
	{
		private readonly FileIndexItemWithIdJsonConverter _elementConverter = new();

		public override List<FileIndexItem> Read(ref Utf8JsonReader reader, Type typeToConvert,
			JsonSerializerOptions options)
		{
			if ( reader.TokenType == JsonTokenType.Null )
			{
				return null!;
			}

			if ( reader.TokenType != JsonTokenType.StartArray )
			{
				throw new JsonException();
			}

			var list = new List<FileIndexItem>();
			reader.Read();
			while ( reader.TokenType != JsonTokenType.EndArray )
			{
				var item = _elementConverter.Read(ref reader, typeof(FileIndexItem), options);
				list.Add(item);
				reader.Read();
			}

			return list;
		}

		public override void Write(Utf8JsonWriter writer, List<FileIndexItem>? value,
			JsonSerializerOptions options)
		{
			if ( value == null )
			{
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartArray();
			foreach ( var item in value )
			{
				_elementConverter.Write(writer, item, options);
			}

			writer.WriteEndArray();
		}
	}

	private sealed class FileIndexItemArrayConverter : JsonConverter<FileIndexItem[]>
	{
		private readonly FileIndexItemWithIdJsonConverter _elementConverter = new();

		public override FileIndexItem[] Read(ref Utf8JsonReader reader, Type typeToConvert,
			JsonSerializerOptions options)
		{
			if ( reader.TokenType == JsonTokenType.Null )
			{
				return null!;
			}

			if ( reader.TokenType != JsonTokenType.StartArray )
			{
				throw new JsonException();
			}

			var list = new List<FileIndexItem>();
			reader.Read();
			while ( reader.TokenType != JsonTokenType.EndArray )
			{
				var item = _elementConverter.Read(ref reader, typeof(FileIndexItem), options);
				list.Add(item);
				reader.Read();
			}

			return list.ToArray();
		}

		public override void Write(Utf8JsonWriter writer, FileIndexItem[]? value,
			JsonSerializerOptions options)
		{
			if ( value == null )
			{
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartArray();
			foreach ( var item in value )
			{
				_elementConverter.Write(writer, item, options);
			}

			writer.WriteEndArray();
		}
	}
}
