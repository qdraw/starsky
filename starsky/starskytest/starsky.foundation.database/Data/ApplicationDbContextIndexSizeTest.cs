using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Data;
using starsky.foundation.database.Models;

namespace starskytest.starsky.foundation.database.Data;

/// <summary>
///     Tests to validate that all database indexes stay under MariaDB's 3072 byte limit
/// </summary>
[TestClass]
public class ApplicationDbContextIndexSizeTest
{
	private const int MaxIndexSizeBytes = 3072;
	private const int Utf8Mb4BytesPerChar = 4; // utf8mb4 uses up to 4 bytes per character

	private static ApplicationDbContext CreateContext()
	{
		var options = new DbContextOptionsBuilder<ApplicationDbContext>()
			.UseInMemoryDatabase(nameof(ApplicationDbContextIndexSizeTest))
			.Options;
		return new ApplicationDbContext(options);
	}

	[TestMethod]
	public void AllIndexes_ShouldBeUnder3072Bytes()
	{
		// Arrange
		using var context = CreateContext();
		var model = context.Model;
		var oversizedIndexes = new List<string>();
		var indexSizes = new Dictionary<string, int>();

		// Act - Check all entity types
		foreach ( var entityType in model.GetEntityTypes() )
		{
			var tableName = entityType.GetTableName();
			if ( string.IsNullOrEmpty(tableName) )
			{
				continue;
			}

			// Check all indexes for this entity
			foreach ( var index in entityType.GetIndexes() )
			{
				var indexName = index.GetDatabaseName() ?? "Unnamed";
				var indexSize = CalculateIndexSize(index);
				indexSizes[$"{tableName}.{indexName}"] = indexSize;

				// The Tags Index is truncated to fit within the index size limit
				if ( indexSize > MaxIndexSizeBytes &&
				     $"{tableName}.{indexName}" != "FileIndexItem.IX_FileIndexItem_Tags" )
				{
					oversizedIndexes.Add(
						$"{tableName}.{indexName}: {indexSize} bytes (exceeds {MaxIndexSizeBytes})");
				}
			}
		}

		// Assert
		if ( oversizedIndexes.Count != 0 )
		{
			var message = "The following indexes exceed MariaDB's 3072 byte limit:\n" +
			              string.Join("\n", oversizedIndexes) + "\n\n" +
			              "All index sizes:\n" +
			              string.Join("\n",
				              indexSizes.OrderByDescending(x => x.Value)
					              .Select(x => $"  {x.Key}: {x.Value} bytes"));

			Assert.Fail(message);
		}

		// Log all index sizes for verification
		Console.WriteLine("All database indexes and their sizes:");
		foreach ( var kvp in indexSizes.OrderByDescending(x => x.Value) )
		{
			Console.WriteLine($"  {kvp.Key}: {kvp.Value} bytes");
		}

		Assert.IsNotEmpty(indexSizes);
	}

	[TestMethod]
	public void FileIndex_ParentDirectory_Index_ShouldBeUnder3072Bytes()
	{
		// Arrange
		using var context = CreateContext();
		var entityType = context.Model.FindEntityType(typeof(FileIndexItem));
		Assert.IsNotNull(entityType, "FileIndexItem entity type should exist");

		// Find the ParentDirectory index
		var parentDirIndex = entityType.GetIndexes()
			.FirstOrDefault(i => i.GetDatabaseName() == "IX_FileIndex_ParentDirectory");

		Assert.IsNotNull(parentDirIndex, "IX_FileIndex_ParentDirectory should exist");

		// Act
		var indexSize = CalculateIndexSize(parentDirIndex);

		// Assert
		Assert.IsLessThanOrEqualTo(MaxIndexSizeBytes,
			indexSize,
			$"IX_FileIndex_ParentDirectory is {indexSize} bytes, exceeds limit of {MaxIndexSizeBytes}");

		Console.WriteLine($"IX_FileIndex_ParentDirectory size: {indexSize} bytes");
	}

	[TestMethod]
	public void Thumbnails_Missing_Index_ShouldBeUnder3072Bytes()
	{
		// Arrange
		using var context = CreateContext();
		var entityType = context.Model.FindEntityType(typeof(ThumbnailItem));
		Assert.IsNotNull(entityType, "ThumbnailItem entity type should exist");

		// Find the missing thumbnails index
		var missingIndex = entityType.GetIndexes()
			.FirstOrDefault(i => i.GetDatabaseName() == "IX_Thumbnails_Missing");

		Assert.IsNotNull(missingIndex, "IX_Thumbnails_Missing should exist");

		// Act
		var indexSize = CalculateIndexSize(missingIndex);

		// Assert
		Assert.IsLessThanOrEqualTo(MaxIndexSizeBytes,
			indexSize,
			$"IX_Thumbnails_Missing is {indexSize} bytes, exceeds limit of {MaxIndexSizeBytes}");

		Console.WriteLine($"IX_Thumbnails_Missing size: {indexSize} bytes");
	}

	[TestMethod]
	public void FileIndex_ParentDirectoryTags_CompositeIndex_WouldExceedLimit()
	{
		// This test documents that ParentDirectory (190) + Tags (1024) would exceed the limit
		// ParentDirectory: varchar(190) = 190 * 4 = 760 bytes
		// Tags: varchar(1024) = 1024 * 4 = 4096 bytes
		// Total: 4856 bytes (exceeds 3072 byte limit)

		const int parentDirMaxLength = 190;
		const int tagsMaxLength = 1024;

		var estimatedSize = parentDirMaxLength * Utf8Mb4BytesPerChar +
		                    tagsMaxLength * Utf8Mb4BytesPerChar;

		Console.WriteLine(
			$"Estimated size of ParentDirectory + Tags composite: {estimatedSize} bytes");

		Assert.IsGreaterThan(MaxIndexSizeBytes, estimatedSize);
	}

	/// <summary>
	///     Calculate the approximate size of an index in bytes for utf8mb4 charset
	/// </summary>
	private static int CalculateIndexSize(IIndex index)
	{
		var totalSize = index.Properties.Sum(CalculatePropertySize);

		// Add some overhead for index metadata (approximately 10-20 bytes)
		totalSize += 16;

		return totalSize;
	}

	/// <summary>
	///     Calculate the size of a single property in an index
	/// </summary>
	private static int CalculatePropertySize(IProperty property)
	{
		var propertyType = property.ClrType;
		var maxLength = property.GetMaxLength();

		// String types
		if ( propertyType == typeof(string) )
		{
			var stringLength = maxLength ?? 255;
			return stringLength * Utf8Mb4BytesPerChar;
		}

		// Get base size for the type
		var baseSize = GetBaseTypeSize(propertyType);
		if ( baseSize > 0 )
		{
			// Add null flag byte for nullable types
			return IsNullableType(propertyType) ? baseSize + 1 : baseSize;
		}

		// Unknown types
		Console.WriteLine(
			$"Warning: Unknown property type {propertyType.Name} in index, estimating 8 bytes");
		return 8;
	}

	/// <summary>
	///     Get the base size for a type (without null flag)
	/// </summary>
	private static int GetBaseTypeSize(Type type)
	{
		var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

		if ( underlyingType == typeof(bool) )
		{
			return 1;
		}

		if ( underlyingType == typeof(int) )
		{
			return 4;
		}

		if ( underlyingType == typeof(long) || underlyingType == typeof(double) ||
		     underlyingType == typeof(DateTime) )
		{
			return 8;
		}

		return underlyingType.IsEnum ? 4 : 0; // Unknown type
	}

	/// <summary>
	///     Check if a type is nullable
	/// </summary>
	private static bool IsNullableType(Type type)
	{
		return Nullable.GetUnderlyingType(type) != null;
	}
}
