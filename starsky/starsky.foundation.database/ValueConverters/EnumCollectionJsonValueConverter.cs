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
		v => string.Join(",",v),
		v => string.IsNullOrEmpty(v) ? new List<T>() : 
			v.Split(",", StringSplitOptions.RemoveEmptyEntries).ToList().Select(x => (T)Enum.Parse(typeof(T), x)).ToList())
	{
	}
}


public class CollectionValueComparer<T> : ValueComparer<ICollection<T>>
{
	public CollectionValueComparer() : base((c1, c2) => c1!.SequenceEqual(c2!),
		c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v!.GetHashCode())), c => c.ToHashSet())
	{
	}
}
