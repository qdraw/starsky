using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.connect.Protocol.Generated;
using starsky.foundation.connect.Sync;

namespace starskytest.starsky.foundation.connect.Sync;

[TestClass]
public sealed class VectorClockTest
{
	private static Vector MakeVector(params (ulong id, ulong value)[] counters)
	{
		var v = new Vector();
		foreach ( var (id, value) in counters )
		{
			v.Counters.Add(new Counter { Id = id, Value = value });
		}

		return v;
	}

	[TestMethod]
	public void Compare_EqualVectors_ReturnsEqual()
	{
		var a = MakeVector((1, 5), (2, 3));
		var b = MakeVector((1, 5), (2, 3));
		Assert.AreEqual(VectorRelation.Equal, VectorClock.Compare(a, b));
	}

	[TestMethod]
	public void Compare_ADominates_WhenAllCountersGreaterOrEqual()
	{
		var a = MakeVector((1, 6), (2, 3));
		var b = MakeVector((1, 5), (2, 3));
		Assert.AreEqual(VectorRelation.ADominates, VectorClock.Compare(a, b));
	}

	[TestMethod]
	public void Compare_BDominates_WhenBAllCountersGreater()
	{
		var a = MakeVector((1, 1));
		var b = MakeVector((1, 2));
		Assert.AreEqual(VectorRelation.BDominates, VectorClock.Compare(a, b));
	}

	[TestMethod]
	public void Compare_Concurrent_WhenNeitherDominates()
	{
		var a = MakeVector((1, 5), (2, 1));
		var b = MakeVector((1, 1), (2, 5));
		Assert.AreEqual(VectorRelation.Concurrent, VectorClock.Compare(a, b));
	}

	[TestMethod]
	public void Compare_EmptyVectors_AreEqual()
	{
		Assert.AreEqual(VectorRelation.Equal, VectorClock.Compare(new Vector(), new Vector()));
	}

	[TestMethod]
	public void Compare_AHasExtraId_ADominates()
	{
		// A has device 2 at value 1; B has no entry for device 2 (treated as 0)
		var a = MakeVector((1, 5), (2, 1));
		var b = MakeVector((1, 5));
		Assert.AreEqual(VectorRelation.ADominates, VectorClock.Compare(a, b));
	}

	[TestMethod]
	public void Increment_AddsNewEntry_WhenDeviceNotPresent()
	{
		var v = new Vector();
		var result = VectorClock.Increment(v, deviceIdShort: 42);
		Assert.AreEqual(1, result.Counters.Count);
		Assert.AreEqual(42ul, result.Counters[0].Id);
		Assert.AreEqual(1ul, result.Counters[0].Value);
	}

	[TestMethod]
	public void Increment_IncrementsExistingEntry()
	{
		var v = MakeVector((42, 3));
		var result = VectorClock.Increment(v, deviceIdShort: 42);
		Assert.AreEqual(4ul, result.Counters[0].Value);
	}

	[TestMethod]
	public void Increment_DoesNotMutateInput()
	{
		var v = MakeVector((42, 3));
		VectorClock.Increment(v, 42);
		Assert.AreEqual(3ul, v.Counters[0].Value, "Original vector must not be modified.");
	}

	[TestMethod]
	public void ConflictName_ContainsSyncConflictMarker()
	{
		var name = VectorClock.ConflictName("photos/img.jpg", "AAAAAAA-BBBBBBB-CCCCCCC-DDDDDDD-EEEEEEE-FFFFFFF-GGGGGGG-HHHHHHH");
		StringAssert.Contains(name, ".sync-conflict-");
		StringAssert.Contains(name, "photos/");
		StringAssert.EndsWith(name, ".jpg");
	}
}
