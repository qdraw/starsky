using System.ComponentModel.DataAnnotations;

namespace starsky.foundation.database.Models;

public static class RotationModel
{
	/// <summary>
	///     Exit Rotation values
	/// </summary>
	public enum Rotation
	{
		DoNotChange = -1,

		// There are more types:
		// https://www.daveperrett.com/articles/2012/07/28/exif-orientation-handling-is-a-ghetto/

		[Display(Name = "Horizontal (normal)")]
		Horizontal = 1,

		[Display(Name = "Rotate 90 CW")] Rotate90Cw = 6,

		[Display(Name = "Rotate 180")] Rotate180 = 3,

		[Display(Name = "Rotate 270 CW")] Rotate270Cw = 8
	}

	public static float ToDegrees(this Rotation rotation)
	{
		// ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
		return rotation switch
		{
			Rotation.Rotate180 => 180,
			Rotation.Rotate90Cw => 90,
			Rotation.Rotate270Cw => 270,
			_ => 0
		};
	}
}
