using System;
using System.Collections.Generic;

namespace starsky.foundation.platform.Models
{
	public static class AccountRoles
	{
		public enum AppAccountRoles
		{
			User,
			Administrator
		}
		
		public static IEnumerable<string> GetAllRoles()
		{
			return Enum.GetNames(typeof(AppAccountRoles));
		}
	}
}
