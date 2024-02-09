using System.Collections.Generic;
using starsky.foundation.database.Models.Account;

namespace starsky.foundation.accountmanagement.Models;

public class UserOverviewModel
{
	public UserOverviewModel(List<User>? objectAllUsersResult = null)
	{
		if ( objectAllUsersResult == null ) return;
		Users = objectAllUsersResult;
		IsSuccess = true;
	}

	public List<User> Users { get; set; } = new List<User>();

	public bool IsSuccess { get; set; }
}
