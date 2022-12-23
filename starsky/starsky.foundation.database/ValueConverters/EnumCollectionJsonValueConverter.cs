using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using starsky.foundation.platform.JsonConverter;

namespace starsky.foundation.database.ValueConverters;
// JsonSerializer.Deserialize
// JsonSerializer.Serialize


/// <summary>
/// Enums as list
/// @see: https://gregkedzierski.com/essays/enum-collection-serialization-in-dotnet-core-and-entity-framework-core/
/// </summary>
/// <typeparam name="T"></typeparam>
public class EnumCollectionJsonValueConverter<T> : ValueConverter<ICollection<T>, string> where T : Enum
{
	public EnumCollectionJsonValueConverter() : base(
		v => JsonSerializer
			.Serialize(v.Select(e => e.ToString()).ToList(), DefaultJsonSerializer.CamelCase),
		v => string.IsNullOrEmpty(v) ? new List<T>() : 
			JsonSerializer.Deserialize<ICollection<string>?>(v, DefaultJsonSerializer.CamelCase)!
			.Select(e => (T) Enum.Parse(typeof(T), e)).ToList())
	{
	}
}


public class CollectionValueComparer<T> : ValueComparer<ICollection<T>>
{
	public CollectionValueComparer() : base((c1, c2) => c1!.SequenceEqual(c2!),
		c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v!.GetHashCode())), c => (ICollection<T>) c.ToHashSet())
	{
	}
}
