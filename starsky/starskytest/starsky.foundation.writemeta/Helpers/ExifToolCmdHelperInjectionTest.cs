using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Helpers;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Services;
using starskytest.FakeMocks;
using ExifToolCmdHelper = starsky.foundation.writemeta.Helpers.ExifToolCmdHelper;

namespace starskytest.starsky.foundation.writemeta.Helpers;

/// <summary>
///     The command is passed to the process as a single `Arguments` string, which .NET splits
///     with the Windows command line rules. A value that is able to close the quote adds
///     its own exiftool switches (for example -config or -@).
/// </summary>
[TestClass]
public sealed class ExifToolCmdHelperInjectionTest
{
	private static readonly string[] ForbiddenSwitches =
	[
		"-config", "-@", "-if", "-p", "-w", "-srcfile", "-tagsfromfile", "-execute"
	];

	/// <summary>
	///     Same rules as the .NET unix process argument parser (Windows style quoting)
	/// </summary>
	private static List<string> SplitArguments(string arguments)
	{
		var result = new List<string>();
		var current = new StringBuilder();
		var inQuotes = false;
		var hasToken = false;
		for ( var i = 0; i < arguments.Length; i++ )
		{
			var c = arguments[i];
			if ( c == '\\' )
			{
				var count = 0;
				while ( i < arguments.Length && arguments[i] == '\\' )
				{
					count++;
					i++;
				}

				hasToken = true;
				if ( i < arguments.Length && arguments[i] == '"' )
				{
					current.Append('\\', count / 2);
					if ( count % 2 == 1 )
					{
						current.Append('"');
					}
					else
					{
						inQuotes = !inQuotes;
					}
				}
				else
				{
					current.Append('\\', count);
					i--;
				}

				continue;
			}

			if ( c == '"' )
			{
				inQuotes = !inQuotes;
				hasToken = true;
				continue;
			}

			if ( ( c == ' ' || c == '\t' || c == '\n' ) && !inQuotes )
			{
				if ( hasToken )
				{
					result.Add(current.ToString());
					current.Clear();
					hasToken = false;
				}

				continue;
			}

			current.Append(c);
			hasToken = true;
		}

		if ( hasToken )
		{
			result.Add(current.ToString());
		}

		return result;
	}

	private static async Task<string> BuildCommand(FileIndexItem model, params string[] names)
	{
		var storage = new FakeIStorage(["/"], ["/test.jpg"], []);
		var sut = new ExifToolCmdHelper(new FakeExifTool(storage, new AppSettings()), storage,
			storage, new FakeReadMeta(), new FakeIThumbnailQuery(), new FakeIWebLogger(),
			new AppSettings());
		var result = await sut.UpdateAsync(model, names.Select(p => p.ToLowerInvariant()).ToList());
		return result.Command;
	}

	private static void AssertNoInjectedSwitch(string command, string payloadName)
	{
		var args = SplitArguments(command);
		var injected = args.Where(p => ForbiddenSwitches.Contains(p)).ToList();
		Assert.IsEmpty(injected,
			$"{payloadName}: injected exiftool switch(es) [{string.Join(", ", injected)}] in: {command}");
	}

	[TestMethod]
	[DataRow(nameof(FileIndexItem.LocationCity), "x\" -config /tmp/evil \"")]
	[DataRow(nameof(FileIndexItem.LocationState), "x\" -@ /tmp/args \"")]
	[DataRow(nameof(FileIndexItem.LocationCountry), "x\" -config /tmp/e \"")]
	[DataRow(nameof(FileIndexItem.LocationCountryCode), "x\" -@ /t\"")]
	[DataRow(nameof(FileIndexItem.Artist), "x\" -config /tmp/evil \"")]
	[DataRow(nameof(FileIndexItem.Software), "x\" -config /tmp/evil \"")]
	[DataRow(nameof(FileIndexItem.Title), "x\\\" -config /tmp/evil \"")]
	[DataRow(nameof(FileIndexItem.Description), "x\\\" -config /tmp/evil \"")]
	[DataRow(nameof(FileIndexItem.Tags), "x\\\" -config /tmp/evil \"")]
	public async Task UpdateAsync_FieldWithQuote_DoesNotInjectExifToolSwitch(string field,
		string payload)
	{
		var model = new FileIndexItem("/test.jpg");
		model.GetType().GetProperty(field)!.SetValue(model, payload);

		var command = await BuildCommand(model, field);

		AssertNoInjectedSwitch(command, field);
	}

	/// <summary>
	///     Same path as POST /api/update: the bound input model is compared with the stored item
	///     (MetaPreflight.CompareAllLabelsAndRotation) and the changed names go to exiftool.
	///     [MaxLength(40)] on the model is validated by MVC, so the payload stays within 40 chars.
	/// </summary>
	[TestMethod]
	[DataRow(nameof(FileIndexItem.LocationCity))]
	[DataRow(nameof(FileIndexItem.Artist))]
	[DataRow(nameof(FileIndexItem.Software))]
	public async Task PreflightCompare_ShortPayload_DoesNotInjectExifToolSwitch(string field)
	{
		const string payload = "x\" -@ /tmp/a \"";
		Assert.IsLessThanOrEqualTo(40, payload.Length);

		var stored = new FileIndexItem("/test.jpg");
		var input = new FileIndexItem("/test.jpg");
		input.GetType().GetProperty(field)!.SetValue(input, payload);

		var changed = FileIndexCompareHelper.Compare(stored, input);
		Assert.Contains(field.ToLowerInvariant(), changed);

		var command = await BuildCommand(stored, [.. changed]);

		AssertNoInjectedSwitch(command, field);
	}

	[TestMethod]
	public async Task UpdateAsync_MakeModelWithQuote_DoesNotInjectExifToolSwitch()
	{
		// Make/Model are parsed from the pipe separated MakeModel field
		var model = new FileIndexItem("/test.jpg")
		{
			// Make|Model|Lens|Serial
			MakeModel = "x\" -config /tmp/evil \"|y\" -@ /tmp/args \"|z\" -config /tmp/l \"|s\" -@ /tmp/s \""
		};

		var command = await BuildCommand(model, nameof(FileIndexItem.MakeModel));

		AssertNoInjectedSwitch(command, nameof(FileIndexItem.MakeModel));
	}
}
