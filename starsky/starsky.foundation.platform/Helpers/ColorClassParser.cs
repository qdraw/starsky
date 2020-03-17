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
			/// Purple
			/// </summary>
			[Display(Name = "Winner")] // <= Display, used in xmp label
			Winner = 1, // Paars - purple
			/// <summary>
			/// Red
			/// </summary>
			[Display(Name = "Winner Alt")]
			WinnerAlt = 2, // rood - Red -
			/// <summary>
			/// Orange
			/// </summary>
			[Display(Name = "Superior")]
			Superior = 3, // Oranje - orange
			/// <summary>
			/// Yellow
			/// </summary>
			[Display(Name = "Superior Alt")]
			SuperiorAlt = 4, //Geel - yellow
			/// <summary>
			/// Green
			/// </summary>
			[Display(Name = "Typical")]
			Typical = 5, // Groen - groen
			/// <summary>
			/// Turquoise
			/// </summary>
			[Display(Name = "Typical Alt")]
			TypicalAlt = 6, // Turquoise
			/// <summary>
			/// Blue
			/// </summary>
			[Display(Name = "Extras")]
			Extras = 7, // Blauw - blue
			/// <summary>
			/// Grey
			/// </summary>
			[Display(Name = "")]
			Trash = 8, // grijs - Grey
			/// <summary>
			/// No color
			/// </summary>
			None = 0, // donkergrijs Dark Grey
			/// <summary>
			/// Option not selected
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
