using System;
using System.Collections.Generic;
using System.Linq;
using starsky.foundation.connect.Protocol.Generated;

namespace starsky.foundation.connect.Sync;

public enum VectorRelation
{
	Equal,
	ADominates,
	BDominates,
	Concurrent,
}

/// <summary>
/// Lamport vector clock operations for BEP v1 conflict detection.
/// </summary>
public static class VectorClock
{
	/// <summary>
	/// Compares two version vectors and returns their relationship.
	/// </summary>
	public static VectorRelation Compare(Vector a, Vector b)
	{
		var allIds = AllIds(a, b);
		var aGreater = false;
		var bGreater = false;

		foreach ( var id in allIds )
		{
			var va = GetCounter(a, id);
			var vb = GetCounter(b, id);
			if ( va > vb )
			{
				aGreater = true;
			}

			if ( vb > va )
			{
				bGreater = true;
			}
		}

		return (aGreater, bGreater) switch
		{
			(false, false) => VectorRelation.Equal,
			(true, false) => VectorRelation.ADominates,
			(false, true) => VectorRelation.BDominates,
			_ => VectorRelation.Concurrent,
		};
	}

	/// <summary>
	/// Increments the counter for <paramref name="deviceIdShort"/> in <paramref name="vector"/>.
	/// Returns a new Vector without mutating the input.
	/// </summary>
	public static Vector Increment(Vector vector, ulong deviceIdShort)
	{
		var result = new Vector();
		var found = false;

		foreach ( var counter in vector.Counters )
		{
			if ( counter.Id == deviceIdShort )
			{
				result.Counters.Add(new Counter { Id = counter.Id, Value = counter.Value + 1 });
				found = true;
			}
			else
			{
				result.Counters.Add(new Counter { Id = counter.Id, Value = counter.Value });
			}
		}

		if ( !found )
		{
			result.Counters.Add(new Counter { Id = deviceIdShort, Value = 1 });
		}

		return result;
	}

	/// <summary>
	/// Returns the conflict filename for a file that was concurrently modified.
	/// Convention: <c>stem.sync-conflict-YYYYMMDD-HHMMSS-DEVICEID.ext</c>
	/// </summary>
	public static string ConflictName(string path, string deviceId)
	{
		var stem = System.IO.Path.GetFileNameWithoutExtension(path);
		var ext = System.IO.Path.GetExtension(path);
		var dir = System.IO.Path.GetDirectoryName(path);
		var stamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
		var shortId = deviceId.Replace("-", string.Empty, StringComparison.Ordinal)[..7];

		var conflictName = $"{stem}.sync-conflict-{stamp}-{shortId}{ext}";
		return dir is { Length: > 0 }
			? System.IO.Path.Combine(dir, conflictName).Replace('\\', '/')
			: conflictName;
	}

	private static ulong GetCounter(Vector v, ulong id)
	{
		foreach ( var c in v.Counters )
		{
			if ( c.Id == id )
			{
				return c.Value;
			}
		}

		return 0;
	}

	private static IEnumerable<ulong> AllIds(Vector a, Vector b)
	{
		return a.Counters.Select(c => c.Id)
			.Concat(b.Counters.Select(c => c.Id))
			.Distinct();
	}
}
