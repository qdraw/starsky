using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Microsoft.Extensions.Caching.Memory;

namespace starsky.foundation.platform.Extensions;

/// <summary>
///     @see: https://stackoverflow.com/a/64291008
/// </summary>
[SuppressMessage("Usage", "S3011:Make sure that this accessibility bypass is safe here",
	Justification = "Safe")]
public static class MemoryCacheExtensions
{
	private static readonly Lazy<Func<MemoryCache, object>>? GetCoherentState =
		new(() =>
			CreateGetter<MemoryCache, object>(typeof(MemoryCache)
				.GetField("_coherentState", BindingFlags.NonPublic | BindingFlags.Instance)!));

	/// <summary>
	///     https://github.com/dotnet/runtime/blob/81cabf2857a01351e5ab578947c7403a5b128ad1/src/libraries/Microsoft.Extensions.Caching.Memory/src/MemoryCache.cs#L680C1-L681C85
	/// </summary>
	private static readonly Lazy<Func<object, IDictionary>> GetEntries7 =
		new(() =>
			CreateGetter<object, IDictionary>(typeof(MemoryCache)
				.GetNestedType("CoherentState", BindingFlags.NonPublic)?
				.GetField("_stringEntries", BindingFlags.NonPublic | BindingFlags.Instance)!));

	private static readonly Func<MemoryCache, IDictionary> GetEntries =
		cache => GetEntries7.Value(GetCoherentState.Value(cache));

	private static Func<TParam, TReturn> CreateGetter<TParam, TReturn>(FieldInfo field)
	{
		var methodName = $"{field.ReflectedType?.FullName}.get_{field.Name}";
		var method = new DynamicMethod(methodName, typeof(TReturn), new[] { typeof(TParam) },
			typeof(TParam), true);
		var ilGen = method.GetILGenerator();
		ilGen.Emit(OpCodes.Ldarg_0);
		ilGen.Emit(OpCodes.Ldfld, field);
		ilGen.Emit(OpCodes.Ret);
		return ( Func<TParam, TReturn> ) method.CreateDelegate(typeof(Func<TParam, TReturn>));
	}

	private static ICollection GetKeys(this IMemoryCache memoryCache)
	{
		return GetEntries(( MemoryCache ) memoryCache).Keys;
	}

	/// <summary>
	///     Get Keys
	/// </summary>
	/// <param name="memoryCache">memory cache</param>
	/// <typeparam name="T">bind as</typeparam>
	/// <returns>list of items</returns>
	public static IEnumerable<T> GetKeys<T>(this IMemoryCache memoryCache)
	{
		try
		{
			return GetKeys(memoryCache).OfType<T>();
		}
		catch ( InvalidCastException )
		{
			return new List<T>();
		}
	}
}
