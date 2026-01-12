using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.rename.Models;
using starsky.feature.rename.Services;
using starsky.foundation.database.Models;

namespace starskytest.starsky.feature.rename.Services;

[TestClass]
public class AssignSequenceNumbersTests
{
	[TestMethod]
	[DynamicData(nameof(GetAssignSequenceNumbersTestCases))]
	public void AssignSequenceNumbers_CoversAllPaths(
		List<BatchRenameMapping> mappings,
		Dictionary<string, FileIndexItem> fileItems,
		List<string> expectedFileNames)
	{
		var pattern = new DummyPattern();
		var service = new RenameService(null!, null!);

		service.AssignSequenceNumbers(mappings, pattern, fileItems);

		var actual = mappings.OrderBy(m => m.SourceFilePath)
			.Select(m => Path.GetFileName(m.TargetFilePath)).ToList();
		CollectionAssert.AreEqual(expectedFileNames, actual);
	}

	public static IEnumerable<object[]> GetAssignSequenceNumbersTestCases()
	{
		// 1. Single file in group
		yield return
		[
			new List<BatchRenameMapping> { new() { SourceFilePath = "/a/file1.jpg" } },
			new Dictionary<string, FileIndexItem>
			{
				["/a/file1.jpg"] = new()
				{
					FileName = "file1.jpg",
					ParentDirectory = "/a",
					DateTime = new DateTime(2020, 1, 1)
				}
			},
			new List<string> { "file1.jpg" }
		];
		// 2. Multiple files, same folder, same base, different extensions
		yield return
		[
			new List<BatchRenameMapping>
			{
				new() { SourceFilePath = "/a/file1.jpg" },
				new() { SourceFilePath = "/a/file1.arw" }
			},
			new Dictionary<string, FileIndexItem>
			{
				["/a/file1.jpg"] =
					new()
					{
						FileName = "file1.jpg",
						ParentDirectory = "/a",
						DateTime = new DateTime(2020, 1, 1)
					},
				["/a/file1.arw"] = new()
				{
					FileName = "file1.arw",
					ParentDirectory = "/a",
					DateTime = new DateTime(2020, 1, 1)
				}
			},
			new List<string> { "file1.jpg", "file1-1.arw" }
		];
		// 3. Multiple files, different folders, same base
		yield return
		[
			new List<BatchRenameMapping>
			{
				new() { SourceFilePath = "/a/file1.jpg" },
				new() { SourceFilePath = "/b/file1.jpg" }
			},
			new Dictionary<string, FileIndexItem>
			{
				["/a/file1.jpg"] =
					new()
					{
						FileName = "file1.jpg",
						ParentDirectory = "/a",
						DateTime = new DateTime(2020, 1, 1)
					},
				["/b/file1.jpg"] = new()
				{
					FileName = "file1.jpg",
					ParentDirectory = "/b",
					DateTime = new DateTime(2020, 1, 1)
				}
			},
			new List<string> { "file1.jpg", "file1.jpg" }
		];
		// 4. Multiple files, same folder, same base, same extension
		yield return
		[
			new List<BatchRenameMapping>
			{
				new() { SourceFilePath = "/a/file1.jpg" },
				new() { SourceFilePath = "/a/file1 (1).jpg" }
			},
			new Dictionary<string, FileIndexItem>
			{
				["/a/file1.jpg"] =
					new()
					{
						FileName = "file1.jpg",
						ParentDirectory = "/a",
						DateTime = new DateTime(2020, 1, 1)
					},
				["/a/file1 (1).jpg"] = new()
				{
					FileName = "file1 (1).jpg",
					ParentDirectory = "/a",
					DateTime = new DateTime(2020, 1, 1)
				}
			},
			new List<string> { "file1.jpg", "file1 (1)-1.jpg" }
		];
		// 5. Multiple groups
		yield return
		[
			new List<BatchRenameMapping>
			{
				new() { SourceFilePath = "/a/file1.jpg" },
				new() { SourceFilePath = "/a/file1.arw" },
				new() { SourceFilePath = "/a/file2.jpg" },
				new() { SourceFilePath = "/a/file2.arw" }
			},
			new Dictionary<string, FileIndexItem>
			{
				["/a/file1.jpg"] =
					new()
					{
						FileName = "file1.jpg",
						ParentDirectory = "/a",
						DateTime = new DateTime(2020, 1, 1)
					},
				["/a/file1.arw"] =
					new()
					{
						FileName = "file1.arw",
						ParentDirectory = "/a",
						DateTime = new DateTime(2020, 1, 1)
					},
				["/a/file2.jpg"] =
					new()
					{
						FileName = "file2.jpg",
						ParentDirectory = "/a",
						DateTime = new DateTime(2020, 1, 2)
					},
				["/a/file2.arw"] = new()
				{
					FileName = "file2.arw",
					ParentDirectory = "/a",
					DateTime = new DateTime(2020, 1, 2)
				}
			},
			new List<string> { "file1.jpg", "file1-1.arw", "file2.jpg", "file2-1.arw" }
		];
		// 6. Order preservation
		yield return
		[
			new List<BatchRenameMapping>
			{
				new() { SourceFilePath = "/a/file1.arw" },
				new() { SourceFilePath = "/a/file1.jpg" }
			},
			new Dictionary<string, FileIndexItem>
			{
				["/a/file1.arw"] =
					new()
					{
						FileName = "file1.arw",
						ParentDirectory = "/a",
						DateTime = new DateTime(2020, 1, 1)
					},
				["/a/file1.jpg"] = new()
				{
					FileName = "file1.jpg",
					ParentDirectory = "/a",
					DateTime = new DateTime(2020, 1, 1)
				}
			},
			new List<string> { "file1.arw", "file1-1.jpg" }
		];
	}

	private class DummyPattern() : RenameTokenPattern("{filenamebase}.{ext}")
	{
		public new string GenerateFileName(FileIndexItem item, int sequence = 0)
		{
			var baseName = item.FileName?.Split('.')[0] ?? "file";
			var ext = Path.GetExtension(item.FileName ?? "");
			return sequence == 0 ? $"{baseName}.{ext}" : $"{baseName}-{sequence}.{ext}";
		}
	}
}
