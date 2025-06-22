using System.ComponentModel.DataAnnotations;

namespace starsky.foundation.platform.Helpers;

public static class ColorClassParser
{
	/// <summary>
	///     ColorClass enum, used to filter images
	///     Display name: used in -xmp:Label
	/// </summary>
	public enum Color
	{
		/// <summary>
		///     Purple (1)
		/// </summary>
		[Display(Name = "Winner")] // <= Display, used in xmp label
		Winner = 1, // Paars - purple

		/// <summary>
		///     Red (2)
		/// </summary>
		[Display(Name = "Winner Alt")] WinnerAlt = 2, // rood - Red -

		/// <summary>
		///     Orange (3)
		/// </summary>
		[Display(Name = "Superior")] Superior = 3, // Oranje - orange

		/// <summary>
		///     Yellow (4)
		/// </summary>
		[Display(Name = "Superior Alt")] SuperiorAlt = 4, //Geel - yellow

		/// <summary>
		///     Green (5)
		/// </summary>
		[Display(Name = "Typical")] Typical = 5, // Groen - groen

		/// <summary>
		///     Turquoise/Azure (6)
		/// </summary>
		[Display(Name = "Typical Alt")] TypicalAlt = 6, // Turquoise

		/// <summary>
		///     Blue (7)
		/// </summary>
		[Display(Name = "Extras")] Extras = 7, // Blauw - blue

		/// <summary>
		///     Grey (8)
		/// </summary>
		[Display(Name = "")] Trash = 8, // grijs - Grey

		/// <summary>
		///     No color (0)
		/// </summary>
		None = 0, // donkergrijs Dark Grey

		/// <summary>
		///     Option not selected (-1)
		/// </summary>
		DoNotChange = -1
	}

	/// <summary>
	///     Use a int value to get the ColorClass enum. The number input is between 1 and 8
	/// </summary>
	/// <param name="colorClassString">The colorclass string.</param>
	/// <returns></returns>
	public static Color GetColorClass(string colorClassString = "0")
	{
		return colorClassString switch
		{
			"0" => Color.None,
			"8" => Color.Trash,
			"7" => Color.Extras,
			"6" => Color.TypicalAlt,
			"5" => Color.Typical,
			"4" => Color.SuperiorAlt,
			"3" => Color.Superior,
			"2" => Color.WinnerAlt,
			"1" => Color.Winner,
			_ => Color.DoNotChange
		};
	}
}
