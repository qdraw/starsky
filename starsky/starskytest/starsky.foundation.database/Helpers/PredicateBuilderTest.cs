using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Helpers;

namespace starskytest.starsky.foundation.database.Helpers;

[TestClass]
public sealed class PredicateBuilderTest
{
	[TestMethod]
	public void PredicateBuilder_IsTrue()
	{
		var expression = PredicateBuilder.True<TestClass>();

		var result = expression.Body.ToString();
		Assert.AreEqual("True", result);
	}

	[TestMethod]
	public void PredicateBuilder_IsFalse()
	{
		var expression = PredicateBuilder.False<TestClass>();

		var result = expression.Body.ToString();
		Assert.AreEqual("False", result);
	}

	[TestMethod]
	public void PredicateBuilder_AndAlso()
	{
		var predicates = new List<Expression<Func<TestClass, bool>>>
		{
			x => x.Key.Contains("Key"),
			x => x.Key.Contains('1'),
			x => x.Key.Contains('2'),
			x => x.Key.Contains('3')
		};

		var predicate = PredicateBuilder.False<TestClass>();

		for ( var i = 0; i < predicates.Count; i++ )
		{
			if ( i == 0 )
			{
				predicate = predicates[i];
			}
			else
			{
				var item = predicates[i - 1];
				var item2 = predicates[i];

				// Search for AND
				predicate = item.AndAlso(item2);
			}
		}

		var example = new List<TestClass>
		{
			new() { Key = "Key" }, new() { Key = "Key12" }, new() { Key = "Key123" }
		};

		var result = example.Where(predicate.Compile()).ToList();

		Assert.AreEqual(1, result.Count);
		Assert.AreEqual("Key123", result[0].Key);
	}

	[TestMethod]
	public void ShouldMatch_And_Criteria()
	{
		var predicates = new List<Expression<Func<TestClass, bool>>>
		{
			x => x.Key.Contains("Key"), x => x.Key.Contains('1')
		};

		var predicate = PredicateBuilder.False<TestClass>();

		for ( var i = 0; i < predicates.Count; i++ )
		{
			if ( i == 0 )
			{
				predicate = predicates[i];
			}
			else
			{
				var item = predicates[i - 1];
				var item2 = predicates[i];

				// Search for AND
				predicate = item.And(item2);
			}
		}

		var example = new List<TestClass> { new() { Key = "Key" }, new() { Key = "Key1" } };

		var result = example.Where(predicate.Compile()).ToList();

		Assert.AreEqual(1, result.Count);
		Assert.AreEqual("Key1", result[0].Key);
	}


	[TestMethod]
	public void ShouldMatch_Or_Criteria()
	{
		var predicates = new List<Expression<Func<TestClass, bool>>>
		{
			x => x.Key.Contains("Key"), x => x.Key.Contains('1')
		};

		var predicate = PredicateBuilder.False<TestClass>();

		for ( var i = 0; i < predicates.Count; i++ )
		{
			if ( i == 0 )
			{
				predicate = predicates[i];
			}
			else
			{
				var item = predicates[i - 1];
				var item2 = predicates[i];
				// Search for OR
				predicate = item.Or(item2);
			}
		}

		var example = new List<TestClass> { new() { Key = "Key" }, new() { Key = "Key12" } };

		var result = example.Where(predicate.Compile()).ToList();

		Assert.AreEqual(2, result.Count);
		Assert.AreEqual("Key", result[0].Key);
		Assert.AreEqual("Key12", result[1].Key);
	}

	private sealed class TestClass
	{
		public string Key { get; set; } = string.Empty;
	}
}
