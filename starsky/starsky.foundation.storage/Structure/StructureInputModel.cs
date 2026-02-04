using System;
using starsky.foundation.platform.Helpers;

namespace starsky.foundation.storage.Structure;

public class StructureInputModel(
	DateTime dateTime,
	string fileNameBase,
	string extensionWithoutDot,
	ExtensionRolesHelper.ImageFormat imageFormat,
	string settingsOrigin)
{
	public DateTime DateTime { get; set; } = dateTime;
	public string FileNameBase { get; set; } = fileNameBase;
	public string ExtensionWithoutDot { get; set; } = extensionWithoutDot;
	public ExtensionRolesHelper.ImageFormat ImageFormat { get; set; } = imageFormat;

	public string Origin { get; set; } = settingsOrigin;
}
