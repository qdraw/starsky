using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace starsky.foundation.writemeta.Helpers
{
	public static class EnumHelper
	{
		/// <summary>
		/// Gets the display name. the value of this field [Display(Name = "Winner")]
		/// </summary>
		/// <param name="enumValue">The enum value.</param>
		/// <returns>display name as string</returns>
		public static string? GetDisplayName(Enum enumValue)
		{
			var name = enumValue?.GetType()
				.GetMember(enumValue.ToString())
				.FirstOrDefault()?
				.GetCustomAttribute<DisplayAttribute>()?
				.Name;
			return name;
		}
	}
}
