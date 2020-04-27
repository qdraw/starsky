using System.ComponentModel.DataAnnotations;

namespace starsky.foundation.platform.Helpers
{
	public static class ColorClassParser
	{
		/// <summary>
		/// ColorClass enum, used to filter images
		/// Display name: used in -xmp:Label
		/// </summary>
		public enum Color
		{
			/// <summary>
			/// Purple (1)
			/// </summary>
			[Display(Name = "Winner")] // <= Display, used in xmp label
			Winner = 1, // Paars - purple
			/// <summary>
			/// Red (2)
			/// </summary>
			[Display(Name = "Winner Alt")]
			WinnerAlt = 2, // rood - Red -
			/// <summary>
			/// Orange (3)
			/// </summary>
			[Display(Name = "Superior")]
			Superior = 3, // Oranje - orange
			/// <summary>
			/// Yellow (4)
			/// </summary>
			[Display(Name = "Superior Alt")]
			SuperiorAlt = 4, //Geel - yellow
			/// <summary>
			/// Green (5)
			/// </summary>
			[Display(Name = "Typical")]
			Typical = 5, // Groen - groen
			/// <summary>
			/// Turquoise (6)
			/// </summary>
			[Display(Name = "Typical Alt")]
			TypicalAlt = 6, // Turquoise
			/// <summary>
			/// Blue (7)
			/// </summary>
			[Display(Name = "Extras")]
			Extras = 7, // Blauw - blue
			/// <summary>
			/// Grey (8)
			/// </summary>
			[Display(Name = "")]
			Trash = 8, // grijs - Grey
			/// <summary>
			/// No color (0)
			/// </summary>
			None = 0, // donkergrijs Dark Grey
			/// <summary>
			/// Option not selected (-1)
			/// </summary>
			DoNotChange = -1
		}
		/// <summary>
		/// Use a int value to get the ColorClass enum. The number input is between 1 and 8
		/// </summary>
		/// <param name="colorclassString">The colorclass string.</param>
		/// <returns></returns>
		public static Color GetColorClass(string colorclassString = "0")
		{

			switch (colorclassString)
			{
				case "0":
					return Color.None;
				case "8":
					return  Color.Trash;
				case "7":
					return Color.Extras;
				case "6":
					return Color.TypicalAlt;
				case "5":
					return Color.Typical;
				case "4":
					return Color.SuperiorAlt;
				case "3":
					return Color.Superior;
				case "2":
					return Color.WinnerAlt;
				case "1":
					return Color.Winner;
				default:
					return Color.DoNotChange;
			}
		}
	}
}
