using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using ModelContextProtocol.Server;
using starsky.project.mcp.Models;
using starsky.project.mcp.Services;
using starsky.feature.metaupdate.Interfaces;
using starsky.feature.trash.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.metaupdate.Interfaces;
using starsky.foundation.platform.Helpers;

namespace starsky.project.mcp.Tools;

/// <summary>
///     Tools that change files. Only registered when McpAllowWrite is true and every call
///     goes through <see cref="McpWriteGuard" />: first a preview, then a confirmed execute
/// </summary>
[McpServerToolType]
public sealed class StarskyWriteTools(
	McpWriteGuard guard,
	IMetaInfo metaInfo,
	IMoveToTrashService moveToTrashService,
	IMetaUpdateRunner metaUpdateRunner,
	McpRenameService renameService)
{
	public const string MoveToTrashTool = "move_to_trash";
	public const string UpdateMetadataTool = "update_metadata";
	public const string RenameTool = "rename";

	[McpServerTool(Name = MoveToTrashTool, ReadOnly = false, Destructive = true,
		Idempotent = false, OpenWorld = false)]
	[Description("Move files to the trash, they can be restored from there. " +
	             "Two steps: the first call changes nothing and returns a preview with a " +
	             "confirmToken. Show the preview to the user, and only after they agree call " +
	             "again with the same arguments and the confirmToken.")]
	public async Task<McpWriteOutcome> MoveToTrash(
		[Description("subPaths of the files, for example `/2024/trip/img_01.jpg`")]
		string[] paths,
		[Description("also move files with the same name before the extension")]
		bool collections = false,
		[Description("token from the preview call, leave empty for the preview")]
		string? confirmToken = null)
	{
		var gate = guard.Gate(MoveToTrashTool, paths, $"collections={collections}",
			confirmToken);

		switch ( gate.Decision )
		{
			case McpGateDecision.Rejected:
				return McpWriteOutcome.Reject(gate.Message);
			case McpGateDecision.NeedsConfirmation:
				var current = await metaInfo.GetInfoAsync(gate.Paths.ToList(), collections);
				return new McpWriteOutcome(McpWriteOutcome.Preview,
					$"{current.Count(p => p.Status == FileIndexItem.ExifStatus.Ok)} file(s) " +
					"will be moved to the trash. Nothing is changed yet.",
					gate.ConfirmToken, McpPhoto.FromItems(current));
		}

		// the system trash removes the file from the source, so the status after the move
		// can't tell moved from never existed: count what existed before
		var existing = ( await metaInfo.GetInfoAsync(gate.Paths.ToList(), collections) )
			.Count(p => p.Status == FileIndexItem.ExifStatus.Ok);
		var moved = await moveToTrashService.CreateEvent(gate.Paths.ToList(), collections);
		guard.Audit(MoveToTrashTool, gate.Paths, $"existed={existing}");
		return new McpWriteOutcome(McpWriteOutcome.Executed,
			$"{existing} existing file(s) moved to the trash.",
			null, McpPhoto.FromItems(moved));
	}

	[SuppressMessage("Major Code Smell", "S107:Methods should not have too many parameters",
		Justification = "Every parameter is an argument of the MCP tool, " +
		                "the agent sees them as the tool's input schema")]
	[McpServerTool(Name = UpdateMetadataTool, ReadOnly = false, Destructive = true,
		Idempotent = true, OpenWorld = false)]
	[Description("Change tags, title, description or color class of files (written to the " +
	             "file and its sidecar). Omitted or empty fields are not changed, so this can't " +
	             "clear a field. Two steps: the first call changes nothing and returns the " +
	             "current values plus a confirmToken. Show the planned change to the user, and " +
	             "only after they agree call again with the same arguments and the confirmToken.")]
	public async Task<McpWriteOutcome> UpdateMetadata(
		[Description("subPaths of the files, for example `/2024/trip/img_01.jpg`")]
		string[] paths,
		[Description("tags, comma separated, for example `holiday, beach`")]
		string? tags = null,
		[Description("image title")] string? title = null,
		[Description("description / caption")] string? description = null,
		[Description("color class 0 (none) to 8, used to mark favorites")]
		int? colorClass = null,
		[Description("add the tags, title and description to the existing values " +
		             "instead of replacing them")]
		bool append = false,
		[Description("also change files with the same name before the extension")]
		bool collections = false,
		[Description("token from the preview call, leave empty for the preview")]
		string? confirmToken = null)
	{
		if ( colorClass is < 0 or > 8 )
		{
			return McpWriteOutcome.Reject("colorClass must be between 0 and 8");
		}

		if ( string.IsNullOrWhiteSpace(tags) && string.IsNullOrWhiteSpace(title) &&
		     string.IsNullOrWhiteSpace(description) && colorClass == null )
		{
			return McpWriteOutcome.Reject("Nothing to change: give tags, title, description " +
			                              "or colorClass");
		}

		var args = $"tags={tags};title={title};description={description};" +
		           $"colorClass={colorClass};append={append};collections={collections}";
		var gate = guard.Gate(UpdateMetadataTool, paths, args, confirmToken);
		if ( gate.Decision == McpGateDecision.Rejected )
		{
			return McpWriteOutcome.Reject(gate.Message);
		}

		if ( gate.Decision == McpGateDecision.NeedsConfirmation )
		{
			var current = await metaInfo.GetInfoAsync(gate.Paths.ToList(), collections);
			return new McpWriteOutcome(McpWriteOutcome.Preview,
				$"{current.Count(p => p.Status == FileIndexItem.ExifStatus.Ok)} file(s) will " +
				$"be changed ({( append ? "append" : "replace" )}): " +
				$"{DescribeChange(tags, title, description, colorClass)}. " +
				"The items show the current values. Nothing is changed yet.",
				gate.ConfirmToken, McpPhoto.FromItems(current));
		}

		var model = new FileIndexItem
		{
			Tags = tags,
			Title = title,
			Description = description,
			ColorClass = colorClass == null
				? ColorClassParser.Color.DoNotChange
				: ( ColorClassParser.Color ) colorClass.Value
		};
		var result = await metaUpdateRunner.UpdateAsync(model, gate.Paths.ToArray(), append,
			collections, 0);
		guard.Audit(UpdateMetadataTool, gate.Paths,
			result.AllNotFound ? "notfound" : "updated");
		if ( result.AllNotFound )
		{
			return new McpWriteOutcome(McpWriteOutcome.Rejected,
				"None of the files was found, nothing is changed.", null,
				McpPhoto.FromItems(result.Results));
		}

		return new McpWriteOutcome(McpWriteOutcome.Executed,
			$"{result.Updated!.Count(p => p.Status == FileIndexItem.ExifStatus.Ok)} " +
			"file(s) updated. The change is written to disk in the background.",
			null, McpPhoto.FromItems(result.Updated!));
	}

	[McpServerTool(Name = RenameTool, ReadOnly = false, Destructive = true,
		Idempotent = false, OpenWorld = false)]
	[Description("Rename or move one file or folder inside the library. It refuses to " +
	             "overwrite. Two steps: the first call changes nothing and returns a " +
	             "confirmToken. Show the planned rename to the user, and only after they " +
	             "agree call again with the same arguments and the confirmToken.")]
	public async Task<McpWriteOutcome> Rename(
		[Description("subPath of the file or folder, for example `/2024/trip/img_01.jpg`")]
		string path,
		[Description("the new subPath, for example `/2024/trip/beach.jpg`")]
		string to,
		[Description("also rename files with the same name before the extension")]
		bool collections = false,
		[Description("token from the preview call, leave empty for the preview")]
		string? confirmToken = null)
	{
		var target = guard.ValidateTarget(to);
		if ( target == null )
		{
			return McpWriteOutcome.Reject($"Target path is not allowed: {to}");
		}

		var gate = guard.Gate(RenameTool, [path], $"to={target};collections={collections}",
			confirmToken);
		if ( gate.Decision == McpGateDecision.Rejected )
		{
			return McpWriteOutcome.Reject(gate.Message);
		}

		var source = gate.Paths[0];
		if ( gate.Decision == McpGateDecision.NeedsConfirmation )
		{
			var current = await metaInfo.GetInfoAsync([source], collections);
			return new McpWriteOutcome(McpWriteOutcome.Preview,
				$"{source} will be renamed to {target}. Nothing is changed yet.",
				gate.ConfirmToken, McpPhoto.FromItems(current));
		}

		var renamed = await renameService.RenameAsync(source, target, collections);
		var success = renamed.Exists(p => p.Status == FileIndexItem.ExifStatus.Ok);
		guard.Audit(RenameTool, gate.Paths,
			$"to={target} {string.Join(',', renamed.Select(p => p.Status))}");
		if ( !success )
		{
			return new McpWriteOutcome(McpWriteOutcome.Rejected,
				"Nothing is renamed, see the status of the items " +
				"(for example the source is missing or the target exists).", null,
				McpPhoto.FromItems(renamed));
		}

		return new McpWriteOutcome(McpWriteOutcome.Executed, $"Renamed to {target}.", null,
			McpPhoto.FromItems(renamed.Where(p =>
				p.Status != FileIndexItem.ExifStatus.NotFoundSourceMissing)));
	}

	private static string DescribeChange(string? tags, string? title, string? description,
		int? colorClass)
	{
		var parts = new List<string>();
		if ( !string.IsNullOrWhiteSpace(tags) )
		{
			parts.Add($"tags `{tags}`");
		}

		if ( !string.IsNullOrWhiteSpace(title) )
		{
			parts.Add($"title `{title}`");
		}

		if ( !string.IsNullOrWhiteSpace(description) )
		{
			parts.Add($"description `{description}`");
		}

		if ( colorClass != null )
		{
			parts.Add($"colorClass {colorClass}");
		}

		return string.Join(", ", parts);
	}
}
