using System.ComponentModel;
using ModelContextProtocol.Server;
using starsky.project.mcp.Models;
using starsky.project.mcp.Services;
using starsky.feature.metaupdate.Interfaces;
using starsky.feature.trash.Interfaces;
using starsky.foundation.database.Models;

namespace starsky.project.mcp.Tools;

/// <summary>
///     Tools that change files. Only registered when McpAllowWrite is true and every call
///     goes through <see cref="McpWriteGuard" />: first a preview, then a confirmed execute
/// </summary>
[McpServerToolType]
public sealed class StarskyWriteTools(
	McpWriteGuard guard,
	IMetaInfo metaInfo,
	IMoveToTrashService moveToTrashService)
{
	public const string MoveToTrashTool = "move_to_trash";

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
				return McpWriteOutcome.Reject(gate.Message ?? "Rejected");
			case McpGateDecision.NeedsConfirmation:
				var current = await metaInfo.GetInfoAsync(gate.Paths.ToList(), collections);
				return new McpWriteOutcome(McpWriteOutcome.Preview,
					$"{current.Count(p => p.Status == FileIndexItem.ExifStatus.Ok)} file(s) " +
					"will be moved to the trash. Nothing is changed yet.",
					gate.ConfirmToken, current.Select(McpPhoto.FromItem).ToList());
		}

		// the system trash removes the file from the source, so the status after the move
		// can't tell moved from never existed: count what existed before
		var existing = ( await metaInfo.GetInfoAsync(gate.Paths.ToList(), collections) )
			.Count(p => p.Status == FileIndexItem.ExifStatus.Ok);
		var moved = await moveToTrashService.CreateEvent(gate.Paths.ToList(), collections);
		guard.Audit(MoveToTrashTool, gate.Paths, $"existed={existing}");
		return new McpWriteOutcome(McpWriteOutcome.Executed,
			$"{existing} existing file(s) moved to the trash.",
			null, moved.Select(McpPhoto.FromItem).ToList());
	}
}
